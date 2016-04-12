using EventCentric.EventSourcing;
using System;

namespace EventCentric.Persistence
{
    public class InMemoryEventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
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
    }
}
