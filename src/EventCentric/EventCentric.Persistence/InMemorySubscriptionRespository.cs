using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Polling;
using System;

namespace EventCentric.Persistence
{
    public class InMemorySubscriptionRespository : ISubscriptionRepository
    {
        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            throw new NotImplementedException();
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            throw new NotImplementedException();
        }
    }
}
