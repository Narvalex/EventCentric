using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class PollerBufferPool : Worker,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpLongPoller http;
        private readonly ITextSerializer serializer;
        private readonly ILogger log;

        /// <summary>
        /// Queue max count for all subscriptons.
        /// </summary>
        private readonly int queueMaxCount;

        /// <summary>
        /// Flush threshold per subscription.
        /// </summary>
        private readonly int eventsToFlushMaxCount;

        private ConcurrentBag<BufferedSubscription> subscriptionsBag;

        private readonly object lockObject = new object();

        public PollerBufferPool(IBus bus, ISubscriptionRepository repository, IHttpLongPoller http, ITextSerializer serializer, ILogger log,
            int queueMaxCount, int eventsToFlushMaxCount)
            : base(bus)
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
            this.subscriptionsBag = this.repository.GetSubscriptions();
            this.log.Trace("Found {0} subscription/s", subscriptionsBag.Count);

            var subscriptionCount = 0;
            foreach (var subscription in this.subscriptionsBag)
            {
                subscriptionCount += 1;
                this.log.Trace(" Subscription {0} | Name: {1} | {1}", subscriptionCount, subscription.StreamType, subscription.Url);
            }
        }

        public bool TryFill()
        {
            var subscriptonsReadyForPolling = this.subscriptionsBag
                                                  .Where(s => !s.IsPolling && !s.IsPoisoned && s.NewEventsQueue.Count < queueMaxCount)
                                                  .ToArray();

            if (!subscriptonsReadyForPolling.Any())
                // all subscriptions are being polled
                return false;

            foreach (var subscription in subscriptonsReadyForPolling)
            {
                subscription.IsPolling = true;
                Task.Factory.StartNewLongRunning(() =>
                    http.PollSubscription(subscription.StreamType, subscription.Url, subscription.CurrentBufferVersion));
            }

            // there are subscriptions that are being polled
            return true;
        }

        public bool TryFlush()
        {
            var subscriptionsReadyToFlush =
                this.subscriptionsBag.Where(s => s.NewEventsQueue.Count > 0 && s.EventsInProcessorBag.All(e => e.WasProcessed))
                                     .ToArray();

            if (!subscriptionsReadyToFlush.Any())
                return false;

            foreach (var subscription in subscriptionsReadyToFlush)
                Task.Factory.StartNewLongRunning(() => this.Flush(subscription));

            return true;
        }

        private void Flush(BufferedSubscription subscription)
        {
#if DEBUG
            var eventsInQueue = subscription.NewEventsQueue.Count();
#endif

            var rawEvents = new List<NewRawEvent>();
            for (int i = 0; i < eventsToFlushMaxCount; i++)
            {
                NewRawEvent rawEvent = null;
                if (!subscription.NewEventsQueue.TryDequeue(out rawEvent))
                    break;

                rawEvents.Add(rawEvent);
            }

            var processorBufferVersion = rawEvents.Min(e => e.EventCollectionVersion);

#if DEBUG
            this.log.Trace("Flushing {0} event/s from {1} queue with {2} event/s", rawEvents.Count, subscription.StreamType, eventsInQueue);
#endif

            // Reset the bag;
            subscription.EventsInProcessorBag = new ConcurrentBag<EventInProcessorBucket>();
            rawEvents.ForEach(raw =>
            {
                var incomingEvent = this.serializer.Deserialize<IEvent>(raw.Payload);

                ((Event)incomingEvent).EventCollectionVersion = raw.EventCollectionVersion;
                ((Event)incomingEvent).ProcessorBufferVersion = processorBufferVersion;

                subscription.EventsInProcessorBag.Add(new EventInProcessorBucket(incomingEvent));

                this.bus.Publish(new NewIncomingEvent(incomingEvent));
            });
        }

        public void Handle(PollResponseWasReceived message)
        {
            var response = message.Response;
            var subscription = this.subscriptionsBag.Where(s => s.StreamType == response.StreamType).Single();

            if (response.NewEventsWereFound)
            {
#if DEBUG
                this.log.Trace("Received {0} event/s", response.NewEvents.Count());
#endif

                var orderedEvents = response.NewEvents.OrderBy(e => e.EventCollectionVersion).ToArray();

                subscription.CurrentBufferVersion = orderedEvents.Last().EventCollectionVersion;

                foreach (var e in orderedEvents)
                    subscription.NewEventsQueue.Enqueue(e);
            }

            subscription.IsPolling = false;
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            this.subscriptionsBag
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
                var poisonedSubscription = this.subscriptionsBag.Where(s => s.StreamType == message.PoisonedEvent.StreamType).Single();
                if (poisonedSubscription.IsPoisoned == true)
                    return;

                poisonedSubscription.IsPoisoned = true;

                this.repository.FlagSubscriptionAsPoisoned(message.PoisonedEvent, message.Exception);

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception)));
            }
        }
    }
}
