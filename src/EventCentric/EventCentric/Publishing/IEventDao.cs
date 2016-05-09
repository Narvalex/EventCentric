using EventCentric.Polling;

namespace EventCentric.Publishing
{

    public interface IEventDao
    {
        NewRawEvent[] FindEvents(long fromEventCollectionVersion, int quantity);

        long GetEventCollectionVersion();
    }
}
