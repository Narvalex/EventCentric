using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    /// <summary>
    /// An event that was hidden for the consumer
    /// </summary>
    public class CloakedEvent : Event
    {
        public static CloakedEvent New(long eventCollectionVersion, string streamType)
        {
            return new CloakedEvent() { EventCollectionVersion = eventCollectionVersion, StreamType = streamType };
        }
    }
}
