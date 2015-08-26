using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class EventBuffer : Worker,
        IMessageHandler<PollResponseWasReceived>
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpPoller http;

        // Sets the threshold where there is need
        private const int bufferMaxThreshold = 100;
        private const int bufferMinThreshold = 50;
        private ConcurrentBag<Subscription> subscriptionsBag;
        private ConcurrentQueue<NewEvent> bufferedEventsQueue;

        public EventBuffer(IBus bus, ISubscriptionRepository repository, IHttpPoller http)
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
            this.bufferedEventsQueue = new ConcurrentQueue<NewEvent>();
            this.subscriptionsBag = this.repository.GetSubscriptions();
        }

        public bool TryFill()
        {
            if (!(bufferedEventsQueue.Count > bufferMinThreshold) && !(bufferedEventsQueue.Count < bufferMinThreshold))
                // The queue if full
                return false;

            var subscriptonsReadyForPolling = this.subscriptionsBag.Where(s => !s.IsPolling).ToArray();
            if (!subscriptonsReadyForPolling.Any())
                // all subscriptions are being polled
                return false;

            foreach (var subscription in subscriptonsReadyForPolling)
            {
                subscription.IsPolling = true;
                Task.Factory.StartNewLongRunning(() => http.PollSubscription(subscription));
            }

            // there are subscriptions that are being polled
            return true;
        }

        public void Handle(PollResponseWasReceived message)
        {
            var response = message.Response;
            if (response.NewEventsWereFound)
            {
                var orderedEvents = response.NewEvents.OrderBy(e => e.EventCollectionVersion).ToArray();

                foreach (var e in orderedEvents)
                    this.bufferedEventsQueue.Enqueue(e);
            }

            this.subscriptionsBag.Where(s => s.StreamType == response.StreamType).Single().IsPolling = false;
        }
    }
}
