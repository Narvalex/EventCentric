using System;

namespace EventCentric.EventSourcing
{
    // Do not chage the public getters in orther to avoid serialization problems
    public class Event : IEvent
    {
        public long EventCollectionVersion { get; set; }

        public Guid TransactionId { get; set; }

        public Guid EventId { get; set; }

        public long ProcessorBufferVersion { get; set; }

        public Guid StreamId { get; set; }

        public string StreamType { get; set; }

        public long Version { get; set; }

        public DateTime LocalTime { get; set; }

        public DateTime UtcTime { get; set; }
    }
}
