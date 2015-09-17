using EventCentric.Transport;
using System.Collections.Generic;

namespace EventCentric.Publishing
{

    public interface IEventDao
    {
        List<NewRawEvent> FindEvents(int fromEventCollectionVersion, int quantity);

        int GetEventCollectionVersion();
    }
}
