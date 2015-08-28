using System;

namespace EventCentric.EventSourcing
{
    public class Event : IEvent
    {
        public int EventCollectionVersion { get; set; }

        public Guid EventId { get; set; }

        public int ProcessorBufferVersion { get; set; }

        public Guid StreamId { get; set; }

        public string StreamType { get; set; }

        public int Version { get; set; }
    }
}
