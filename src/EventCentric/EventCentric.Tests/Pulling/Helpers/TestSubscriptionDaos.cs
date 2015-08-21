using EventCentric.Pulling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestSubscriptionDaoWithSingleResult : ISubscriptionDao
    {
        public static List<SubscribedStream> Subscriptions;

        public TestSubscriptionDaoWithSingleResult(Guid streamId)
        {
            Subscriptions = new List<SubscribedStream>
            {
                new SubscribedStream(
                    isPoisoned: false,
                    streamId: streamId,
                    streamType: "Clients",
                    version: 0)
            };
        }

        public ConcurrentBag<SubscribedSource> GetSubscribedSources()
        {
            throw new NotImplementedException();
        }

        ConcurrentBag<SubscribedStream> ISubscriptionDao.GetSubscribedStreamsOrderedByStreamName()
        {
            return new ConcurrentBag<SubscribedStream>(Subscriptions);
        }
    }
}
