using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    /// <summary>
    /// An event that was hidden for the consumer
    /// </summary>
    public class CloakedEvent : Event
    {
        public static CloakedEvent New(Guid eventId, long eventCollectionVersion, string streamType)
        {
            return new CloakedEvent()
            {
                EventId = eventId,
                EventCollectionVersion = eventCollectionVersion,
                StreamType = streamType
            };
        }
    }
}
