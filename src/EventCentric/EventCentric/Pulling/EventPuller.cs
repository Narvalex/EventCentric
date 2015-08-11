using System;
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
                    var batch = 0;
                    foreach (var subscription in pendingSubscriptions)
                    {
                        // Mark subscription as busy
                        batch += 1;
                        subscription.IsBusy = true;
                        if (batch > 9)
                        {
                            // Send request async.
                            batch = 0;
                            TryPullFromRemote();
                        }
                    }
                }
            }
        }

        private void TryPullFromRemote()
        {
            // This is an asyncronous method. In a separate thread we make a new request for the batch of subscriptions that we have
            // for each stream type.

            // If failure or no new event, the subscription is not busy anymore
            var failedId = Guid.Empty;
            this.subscriptions.Where(s => s.StreamId == failedId).First().IsBusy = false;

            // If success, then we publish to the bus that a new message was found;
            // The processor will notify that the message was processed an will update the version, or notify that is poisoned after
            //  a few retries. 
        }
    }
}
