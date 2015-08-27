using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class BufferPool : Worker,
        IMessageHandler<PollResponseWasReceived>
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpPoller http;

        // Sets the threshold where there is need
        private const int bufferMinThreshold = 50;
        private ConcurrentBag<BufferedSubscription> subscriptionsBag;

        public BufferPool(IBus bus, ISubscriptionRepository repository, IHttpPoller http)
            : base(bus)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(http, "http");

            this.repository = repository;
            this.http = http;
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
                                                  .Where(s => !s.IsPolling && s.NewEventsQueue.Count < 50)
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
            return false;
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
    }
}
