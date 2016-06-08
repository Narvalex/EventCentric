using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    /// <summary>
    /// An event that was hidden for the consumer
    /// </summary>
    public class CloakedEvent : Event
    {
        public CloakedEvent(long eventCollectionVersion, string streamType)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.StreamType = streamType;
        }
    }
}
