using EventCentric.Repository;
using System;
using System.Collections.Generic;
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

        public IEnumerable<Subscription> GetSubscriptionsOrderedByStreamName()
        {
            try
            {
                using (var context = this.contextFactory.Invoke())
                {
                    return context
                            .Subscriptions
                            .OrderBy(s => s.StreamType)
                            .Select(s => new Subscription(s.StreamType, s.StreamId, s.Url, s.LastProcessedVersion, s.IsPoisoned))
                            .AsEnumerable();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
