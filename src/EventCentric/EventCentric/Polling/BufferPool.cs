using EventCentric.EventSourcing;
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
    public class BufferPool : Worker,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpPoller http;
        private readonly ITextSerializer serializer;

        // Sets the threshold where there is need
        private const int queueMaxThreshold = 100;
        private const int eventsToFlushMaxCount = 50;
        private ConcurrentBag<BufferedSubscription> subscriptionsBag;

        public BufferPool(IBus bus, ISubscriptionRepository repository, IHttpPoller http, ITextSerializer serializer)
            : base(bus)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(http, "http");
            Ensure.NotNull(serializer, "serializer");

            this.repository = repository;
            this.http = http;
            this.serializer = serializer;
        }

        /// <summary>
        /// Pulls the Event Collection Version for each subscripton.
        /// </summary>
        public void Initialize()
        {
            this.subscriptionsBag = this.repository.GetSubscriptions();
        }

        public bool TryFill()
        {
            var subscriptonsReadyForPolling = this.subscriptionsBag
                                                  .Where(s => !s.IsPolling && s.NewEventsQueue.Count < queueMaxThreshold)
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
                this.subscriptionsBag.Where(s => s.NewEventsQueue.Count > 0 && s.EventsInProcessorBag.IsEmpty)
                                     .ToArray();

            if (!subscriptionsReadyToFlush.Any())
                return false;

            foreach (var subscription in subscriptionsReadyToFlush)
            {
                var rawEvents = new List<NewRawEvent>();
                for (int i = 0; i < eventsToFlushMaxCount; i++)
                {
                    NewRawEvent rawEvent = null;
                    if (!subscription.NewEventsQueue.TryDequeue(out rawEvent))
                        break;

                    rawEvents.Add(rawEvent);
                }

                var processorBufferVersion = rawEvents.Min(e => e.EventCollectionVersion);
                rawEvents.ForEach(raw =>
                {
                    var incomingEvent =
                        new IncomingEvent<IEvent>(
                            raw.EventCollectionVersion,
                            processorBufferVersion,
                            this.serializer.Deserialize<IEvent>(raw.Payload));

                    subscription.EventsInProcessorBag.Add(incomingEvent);

                    this.bus.Publish(new NewIncomingEvent(incomingEvent));
                });
            }

            return true;
        }

        public void Handle(PollResponseWasReceived message)
        {
            var response = message.Response;
            var subscription = this.subscriptionsBag.Where(s => s.StreamType == response.StreamType).Single();

            if (response.NewEventsWereFound)
            {
                var orderedEvents = response.NewEvents.OrderBy(e => e.EventCollectionVersion).ToArray();

                subscription.CurrentBufferVersion = orderedEvents.Last().EventCollectionVersion;

                foreach (var e in orderedEvents)
                    subscription.NewEventsQueue.Enqueue(e);
            }

            subscription.IsPolling = false;
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {

        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            this.repository.FlagSubscriptionAsPoisoned(message.PoisonedEvent);

            this.bus.Publish(
                new FatalErrorOcurred(
                    new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception)));
        }
    }
}
