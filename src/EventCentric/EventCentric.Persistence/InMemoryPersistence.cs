using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Publishing;
using System;
using System.Collections.Generic;

namespace EventCentric.Persistence
{
    public class InMemoryPersistence<T> :
        ISubscriptionRepository,
        IEventDao,
        IEventStore<T> where T : class, IEventSourced
    {
        #region EventDao
        public List<NewRawEvent> FindEvents(long fromEventCollectionVersion, int quantity)
        {
            throw new NotImplementedException();
        }

        public long GetEventCollectionVersion()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region SubscriptionRepository
        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            throw new NotImplementedException();
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region EventStore
        public void DeleteSnapshot(Guid streamId)
        {
            throw new NotImplementedException();
        }

        public T Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public T Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool IsDuplicate(Guid eventId)
        {
            throw new NotImplementedException();
        }

        public long Save(T eventSourced, IEvent incomingEvent)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
