using EventCentric.Pulling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestSubscriptionDaoWithSingleResult : ISubscriptionDao
    {
        public static List<Subscription> Subscriptions;

        public TestSubscriptionDaoWithSingleResult(Guid streamId)
        {
            Subscriptions = new List<Subscription>
            {
                new Subscription(
                    isPoisoned: false,
                    streamId: streamId,
                    streamType: "Clients",
                    url: "http://www.google.com",
                    version: 0)
            };
        }

        ConcurrentBag<Subscription> ISubscriptionDao.GetSubscriptionsOrderedByStreamName()
        {
            return new ConcurrentBag<Subscription>(Subscriptions);
        }
    }
}
