using EventCentric.Polling;
using EventCentric.Publishing;
using System;
using System.Collections.Generic;

namespace EventCentric.Persistence
{
    public class InMemoryEventDao : IEventDao
    {
        public List<NewRawEvent> FindEvents(long fromEventCollectionVersion, int quantity)
        {
            throw new NotImplementedException();
        }

        public long GetEventCollectionVersion()
        {
            throw new NotImplementedException();
        }
    }
}
