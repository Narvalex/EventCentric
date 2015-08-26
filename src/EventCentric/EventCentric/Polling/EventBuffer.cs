using EventCentric.Pulling;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class EventBuffer
    {
        private readonly ISubscriptionRepository repository;
        private readonly IHttpPoller http;

        // Sets the threshold where there is need
        private const int bufferMaxThreshold = 100;
        private const int bufferMinThreshold = 50;
        private ConcurrentBag<Subscription> subscriptionsBag;
        private ConcurrentQueue<object> bufferedEventsQueue;

        public EventBuffer(ISubscriptionRepository repository, IHttpPoller http)
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
            this.bufferedEventsQueue = new ConcurrentQueue<object>();
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
    }
}
