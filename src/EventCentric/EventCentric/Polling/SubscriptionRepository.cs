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

        public ConcurrentBag<Subscription> GetSubscriptions()
        {
            var subscriptions = new ConcurrentBag<Subscription>();
            using (var context = this.contextFactory())
            {
                var subscriptionsQuery = context.Subscriptions.Where(s => !s.IsPoisoned);
                if (subscriptionsQuery.Any())
                    foreach (var s in subscriptionsQuery)
                        subscriptions.Add(new Subscription(s.StreamType, s.Url, s.EventsInProcessorVersion, s.EventCollectionVersion));

                return subscriptions;
            }
        }
    }
}
