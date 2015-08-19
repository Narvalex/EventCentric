using EventCentric.Repository;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Pulling
{
    public class SubscriptionDao : ISubscriptionDao
    {
        private readonly Func<ReadOnlySubscriptionDbContext> contextFactory;

        public SubscriptionDao(Func<ReadOnlySubscriptionDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public ConcurrentBag<Subscription> GetSubscriptionsOrderedByStreamName()
        {
            var subscriptionsInBag = new ConcurrentBag<Subscription>();
            using (var context = this.contextFactory.Invoke())
            {
                var subscriptions = context
                                     .Subscriptions
                                     .OrderBy(s => s.StreamType)
                                     .AsCachedAnyEnumerable();

                if (subscriptions.Any())
                {
                    foreach (var s in subscriptions)
                        subscriptionsInBag.Add(new Subscription(s.StreamType, s.StreamId, s.Url, s.LastProcessedVersion, s.IsPoisoned));
                }

                return subscriptionsInBag;
            }
        }
    }
}
