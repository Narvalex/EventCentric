using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EventCentric.Pulling
{
    public class EventPuller
    {
        private readonly ConcurrentBag<Subscription> subscriptions;

        public EventPuller()
        {
            // Pull from db.
            this.subscriptions = new ConcurrentBag<Subscription>();
        }

        public void Pull()
        {
            while (true)
            {
                var pendingSubscriptions = subscriptions.Where(s => !s.IsBusy && !s.IsPoisoned);

                if (pendingSubscriptions.Count() == 0)
                    Thread.Sleep(100);
                else
                {
                    // Build request.

                    foreach (var subscription in pendingSubscriptions)
                    {
                        // Mark subscription as busy
                        subscription.IsBusy = true;
                    }

                    // Send request.
                    TryPullFromRemote();
                }
            }
        }

        private void TryPullFromRemote()
        {
        }
    }
}
