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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class Poller : MicroserviceWorker, IMonitoredSubscriber,
        IMessageHandler<StartEventPoller>,
        IMessageHandler<StopEventPoller>,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly ISubscriptionRepository repository;
        private readonly ILongPoller poller;
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

        public Poller(IBus bus, ILogger log, ISubscriptionRepository repository, ILongPoller poller, ITextSerializer serializer,
            int queueMaxCount, int eventsToFlushMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(poller, nameof(poller));
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(log, "logger");

            Ensure.Positive(queueMaxCount, "queueMaxCount");
            Ensure.Positive(eventsToFlushMaxCount, "eventsToFlushMaxCount");

            this.repository = repository;
            this.poller = poller;
            this.serializer = serializer;
            this.log = log;

            this.queueMaxCount = queueMaxCount;
            this.eventsToFlushMaxCount = eventsToFlushMaxCount;
        }

        /// <summary>
        /// Pulls the Event Collection Version for each subscripton.
        /// </summary>
        private void Initialize()
        {
            this.bufferPool = this.repository.GetSubscriptions();
            this.log.Trace("Found {0} subscription/s", bufferPool.Count());

            var subscriptionCount = 0;
            foreach (var subscription in this.bufferPool)
            {
                subscriptionCount += 1;
                this.log.Trace($" Subscription {subscriptionCount} | Name: {subscription.StreamType} | {subscription.Url} | Buffer version: {subscription.CurrentBufferVersion}");
            }
        }

        private bool TryFill()
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
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(_ =>
                    poller.PollSubscription(subscription.StreamType, subscription.Url, subscription.Token, subscription.CurrentBufferVersion)), null);
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

            // Reset the bag;
            buffer.EventsInProcessorBag = new ConcurrentBag<EventInProcessorBucket>();
            var streams = rawEvents.Select(raw =>
            {
                IEvent incomingEvent;

                try
                {
                    incomingEvent = this.serializer.Deserialize<IEvent>(raw.Payload);
                }
#if !DEBUG
                catch (SerializationException)
                {
#endif
#if DEBUG
                catch (SerializationException ex)
                {
                    // Maybe the event source has new events type that we are not aware off.

                    this.log.Error(ex, "An error ocurred while deserializing a message");
                    this.log.Trace($"An error was detected when serializing a message from {buffer.ProducerName} with event collection number of {raw.EventCollectionVersion}. The message will be ignored.");
#endif
                    incomingEvent = new Message();
                }
                catch (Exception ex)
                {
                    var errorMessage = $"An error occurred while trying to deserialize a payload from an incoming event. Check the inner exception for more information. System will shut down.";
                    this.log.Error(ex, errorMessage);

                    this.bus.Publish(
                        new FatalErrorOcurred(
                            new FatalErrorException(errorMessage, ex)));

                    throw;
                }

                ((Message)incomingEvent).EventCollectionVersion = raw.EventCollectionVersion;
                ((Message)incomingEvent).ProcessorBufferVersion = processorBufferVersion;

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

            this.log.Trace($"Flushing {rawEvents.Count} event/s of {buffer.StreamType} queue with {eventsInQueueCount} event/s pulled from {buffer.Url}");

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
                var errorMessage = $@"An error occurred while receiving poll response message. Check if the stream type matches that of the subscribed source. Remote message type that probably was not found is '{response.StreamType}'";
                this.log.Error(ex, errorMessage);

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException(errorMessage, ex)));

                throw;
            }

            if (response.NewEventsWereFound)
            {
                this.log.Trace("Received {0} event/s from {1}", response.NewRawEvents.Count(), subscription.StreamType);

                var orderedEvents = response.NewRawEvents.OrderBy(e => e.EventCollectionVersion).ToArray();

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
                try
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
                catch (Exception ex)
                {
                    this.bus.Publish(
                        new FatalErrorOcurred(
                            new FatalErrorException($"An error ocurred while attempting to log poisoned message. {ex.Message}",
                                new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception))));
                }
            }
        }

        public void Handle(StopEventPoller message)
        {
            base.Stop();
        }

        public void Handle(StartEventPoller message)
        {
            base.Start();
        }

        protected override void OnStarting()
        {
            this.Initialize();
            Task.Factory.StartNewLongRunning(() => this.KeepTheBufferFull());
            this.DispatchEventsFromBufferPool();

            // Ensure to start everything;
            this.log.Trace("Poller started");
            this.bus.Publish(new EventPollerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.log.Trace("Poller stopped");
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
                            Thread.Sleep(1);
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
                    Thread.Sleep(1);
            }
        }

        public IMonitoredSubscription[] GetSubscriptionsMetrics() => this.bufferPool;

        public List<SubscriptionBuffer> GetBufferPool()
        {
            var pool = new List<SubscriptionBuffer>();
            pool.AddRange(this.bufferPool);
            return pool;
        }

        public void StopSilently()
        {
            this.stopping = true;
        }
    }
}
