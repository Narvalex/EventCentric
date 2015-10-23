using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class Poller : FSM, IMonitoredSubscriber,
        IMessageHandler<StartEventPoller>,
        IMessageHandler<StopEventPoller>,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpLongPoller http;
        private readonly ITextSerializer serializer;

        /// <summary>
        /// Queue max count for all subscriptons.
        /// </summary>
        private readonly int queueMaxCount;

        /// <summary>
        /// Flush threshold per subscription.
        /// </summary>
        private readonly int eventsToFlushMaxCount;

        private SubscriptionBuffer[] bufferPool;

        private readonly object lockObject = new object();

        public Poller(IBus bus, ILogger log, ISubscriptionRepository repository, IHttpLongPoller http, ITextSerializer serializer,
            int queueMaxCount, int eventsToFlushMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(http, "http");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(log, "logger");

            Ensure.Positive(queueMaxCount, "queueMaxCount");
            Ensure.Positive(eventsToFlushMaxCount, "eventsToFlushMaxCount");

            this.repository = repository;
            this.http = http;
            this.serializer = serializer;
            this.log = log;

            this.queueMaxCount = queueMaxCount;
            this.eventsToFlushMaxCount = eventsToFlushMaxCount;
        }

        /// <summary>
        /// Pulls the Event Collection Version for each subscripton.
        /// </summary>
        public void Initialize()
        {
            this.bufferPool = this.repository.GetSubscriptions();
            this.log.Trace("Found {0} subscription/s", bufferPool.Count());

            var subscriptionCount = 0;
            foreach (var subscription in this.bufferPool)
            {
                subscriptionCount += 1;
                this.log.Trace(" Subscription {0} | Name: {1} | {1}", subscriptionCount, subscription.StreamType, subscription.Url);
            }
        }

        public bool TryFill()
        {
            var subscriptonsReadyForPolling = this.bufferPool
                                                  .Where(s => !s.IsPolling && !s.IsPoisoned && s.NewEventsQueue.Count < queueMaxCount)
                                                  .ToArray();

            if (!subscriptonsReadyForPolling.Any())
                // all subscriptions are being polled
                return false;

            foreach (var subscription in subscriptonsReadyForPolling)
            {
                subscription.IsPolling = true;
                Task.Factory.StartNewLongRunning(() =>
                    http.PollSubscription(subscription.StreamType, subscription.Url, subscription.Token, subscription.CurrentBufferVersion));
            }

            // there are subscriptions that are being polled
            return true;
        }

        private bool TryFlush(SubscriptionBuffer buffer)
        {
            var eventsInQueueCount = buffer.NewEventsQueue.Count();
            if (eventsInQueueCount < 1 || buffer.EventsInProcessorBag.Any(e => !e.WasProcessed))
                // The buffer is empty or there are still events in the processor
                return false;

            var rawEvents = new List<NewRawEvent>();
            for (int i = 0; i < eventsToFlushMaxCount; i++)
            {
                NewRawEvent rawEvent = null;
                if (!buffer.NewEventsQueue.TryDequeue(out rawEvent))
                    break;

                rawEvents.Add(rawEvent);
            }

            var processorBufferVersion = rawEvents.Min(e => e.EventCollectionVersion);

#if DEBUG
            this.log.Trace("Flushing {0} event/s of {1} queue with {2} event/s pulled from {3}", rawEvents.Count, buffer.StreamType, eventsInQueueCount, buffer.Url);
#endif

            // Reset the bag;
            buffer.EventsInProcessorBag = new ConcurrentBag<EventInProcessorBucket>();
            var streams = rawEvents.Select(raw =>
            {
                var incomingEvent = this.serializer.Deserialize<IEvent>(raw.Payload);

                ((Event)incomingEvent).EventCollectionVersion = raw.EventCollectionVersion;
                ((Event)incomingEvent).ProcessorBufferVersion = processorBufferVersion;

                buffer.EventsInProcessorBag.Add(new EventInProcessorBucket(incomingEvent));

                return incomingEvent;
                //this.bus.Publish(new NewIncomingEvents(incomingEvent));
            })
            .GroupBy(x => x.StreamId, x => x,
                    (key, group) => new
                    {
                        StreamId = key,
                        Events = group.ToList()
                    });

            foreach (var stream in streams)
                this.bus.Publish(new NewIncomingEvents(stream.Events.OrderBy(x => x.Version).ToArray()));

            return true;
        }

        public void Handle(PollResponseWasReceived message)
        {
            var response = message.Response;

            SubscriptionBuffer subscription;
            try
            {
                subscription = this.bufferPool.Where(s => s.StreamType == response.StreamType).Single();
            }
            catch (Exception ex)
            {
                var errorMessage = $@"An error ocureed while receiving poll response message. Check if the stream type matches that of the subscribed source. Remote message type that probably was not found is '{response.StreamType}'";
                this.log.Error(ex, errorMessage);

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException(errorMessage, ex)));

                throw;
            }

            if (response.NewEventsWereFound)
            {
                this.log.Trace("Received {0} event/s from {1}", response.NewEvents.Count(), subscription.StreamType);

                var orderedEvents = response.NewEvents.OrderBy(e => e.EventCollectionVersion).ToArray();

                subscription.CurrentBufferVersion = orderedEvents.Max(e => e.EventCollectionVersion);

                foreach (var e in orderedEvents)
                {
                    //this.log.Trace("Event: ECVersion {0}", e.EventCollectionVersion);
                    subscription.NewEventsQueue.Enqueue(e);
                }
            }

            if (!response.ErrorDetected)
            {
                subscription.ConsumerVersion = response.ConsumerVersion;
                subscription.ProducerVersion = response.ProducerVersion;
            }

            subscription.IsPolling = false;
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            this.bufferPool
                    .Where(s => s.StreamType == message.StreamType)
                    .Single()
                        .EventsInProcessorBag
                        .Where(e => e.Event.EventCollectionVersion == message.EventCollectionVersion)
                        .Single()
                        .MarkEventAsProcessed();
        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            lock (this.lockObject)
            {
                var poisonedSubscription = this.bufferPool.Where(s => s.StreamType == message.PoisonedEvent.StreamType).Single();
                if (poisonedSubscription.IsPoisoned == true)
                    return;

                poisonedSubscription.IsPoisoned = true;

                this.repository.FlagSubscriptionAsPoisoned(message.PoisonedEvent, message.Exception);

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception)));
            }
        }

        public void Handle(StopEventPoller message)
        {
            this.log.Trace("Stopping poller");
            base.Stop();
            this.log.Trace("Poller stopped");
        }

        public void Handle(StartEventPoller message)
        {
            this.log.Trace("Starting poller");
            base.Start();
        }

        protected override void OnStarting()
        {
            this.Initialize();
            Task.Factory.StartNewLongRunning(() => this.KeepTheBufferFull());
            Task.Factory.StartNewLongRunning(() => this.DispatchEventsFromBufferPool());

            // Ensure to start everything;
            this.log.Trace("Pollster started");
            this.bus.Publish(new EventPollerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPollerStopped());
        }

        private void DispatchEventsFromBufferPool()
        {
            foreach (var buffer in this.bufferPool)
            {
                Task.Factory.StartNewLongRunning(() =>
                {
                    while (!base.stopping)
                    {
                        var isStarving = !this.TryFlush(buffer);

                        if (isStarving)
                            Thread.Sleep(100);
                    }
                });
            }
        }

        private void KeepTheBufferFull()
        {
            while (!base.stopping)
            {
                var isStarving = !this.TryFill();

                if (isStarving)
                    Thread.Sleep(100);
            }
        }

        public IMonitoredSubscription[] GetSubscriptionsMetrics()
        {
            return this.bufferPool;
        }
    }
}
