using EventCentric.Repository;
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

        public ConcurrentBag<SubscribedSource> GetSubscribedSources()
        {
            var subscribedSources = new ConcurrentBag<SubscribedSource>();
            using (var context = contextFactory())
            {
                var sources = context.SubscribedSources;

                if (sources.Any())
                    foreach (var s in sources)
                        subscribedSources.Add(new SubscribedSource(s.StreamType, s.Url, s.StreamCollectionVersion));

                return subscribedSources;
            }
        }

        public ConcurrentBag<SubscribedStream> GetSubscribedStreamsOrderedByStreamName()
        {
            var subscriptionsInBag = new ConcurrentBag<SubscribedStream>();
            using (var context = this.contextFactory.Invoke())
            {
                var subscriptions = context.Subscriptions;

                if (subscriptions.Any())
                    // Subscripion for existing streams
                    foreach (var s in subscriptions.OrderBy(s => s.StreamType))
                        subscriptionsInBag.Add(
                            new SubscribedStream(s.StreamType, s.StreamId, s.LastProcessedVersion, s.IsPoisoned));

                return subscriptionsInBag;
            }
        }
    }
}
