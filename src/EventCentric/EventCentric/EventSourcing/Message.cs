using System;

namespace EventCentric.EventSourcing
{
    /// <summary>
    /// Every message in the system is actually an event. Commands and events are events.
    /// But still we call it a message to avoid confusion whether a message is a command or an event.
    /// </summary>
    public class Message : IEvent
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

        public bool IsACommand { get; protected set; }
    }
}
