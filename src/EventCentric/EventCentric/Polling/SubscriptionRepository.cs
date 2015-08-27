using EventCentric.Repository;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Polling
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly Func<EventStoreDbContext> contextFactory;

        public SubscriptionRepository(Func<EventStoreDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public ConcurrentBag<BufferedSubscription> GetSubscriptions()
        {
            var subscriptions = new ConcurrentBag<BufferedSubscription>();
            using (var context = this.contextFactory())
            {
                var subscriptionsQuery = context.Subscriptions.Where(s => !s.IsPoisoned);
                if (subscriptionsQuery.Any())
                    foreach (var s in subscriptionsQuery)
                        // We substract one version in order to set the current version bellow the last one, in case that first event
                        // was not yet processed.
                        subscriptions.Add(new BufferedSubscription(s.StreamType, s.Url, s.ProcessorBufferVersion - 1));

                return subscriptions;
            }
        }
    }
}
