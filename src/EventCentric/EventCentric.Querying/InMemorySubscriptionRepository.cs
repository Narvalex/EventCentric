using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Polling;
using System;
using System.Collections.Generic;

namespace EventCentric.Querying
{
    public class InMemorySubscriptionRepository : ISubscriptionRepository
    {
        private readonly List<SubscriptionBuffer> subscriptions = new List<SubscriptionBuffer>();

        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            throw new InvalidOperationException("An error ocurred while trying to query a node");
        }

        public void RegisterSubscriptions(params EventSourceConnection[] connections)
        {
            foreach (var connection in connections)
                this.subscriptions.Add(new SubscriptionBuffer(connection.StreamType, connection.Url, connection.Token, 0, false));
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            return this.subscriptions.ToArray();
        }

        public bool TryAddNewSubscriptionOnTheFly(string streamType, string url, string token)
        {
            throw new NotImplementedException();
        }

        public void PersistSubscriptionVersion(string subscription, long version)
        {
            throw new NotImplementedException();
        }

        public void PersistSubscriptionVersion(string subscription, long consumerVersion, long producerVersion)
        {
            throw new NotImplementedException();
        }
    }
}
