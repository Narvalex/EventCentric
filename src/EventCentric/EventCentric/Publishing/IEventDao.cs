using EventCentric.Transport;
using System.Collections.Generic;

namespace EventCentric.Publishing
{

    public interface IEventDao
    {
        List<NewRawEvent> FindEvents(long fromEventCollectionVersion, int quantity);

        long GetEventCollectionVersion();
    }
}
