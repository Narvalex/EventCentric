using System;

namespace EventCentric.EventSourcing
{
    public interface IEvent
    {
        Guid TransactionId { get; }

        /// <summary>
        /// The id of the event.
        /// </summary>
        Guid EventId { get; }

        string StreamType { get; }

        Guid StreamId { get; }

        /// <summary>
        /// The version of the stream when the event happened.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// The version of the event from the store perspective.
        /// </summary>
        long EventCollectionVersion { get; }

        /// <summary>
        /// The lowest version of the event collection version that the processor was consuming from source. This 
        /// is useful in orther to resume an interrupted polling, for instance, when the system restarts.
        /// </summary>
        long ProcessorBufferVersion { get; }

        /// <summary>
        /// A hint that lets the system knows that the message is a command.
        /// </summary>
        bool IsACommand { get; }

        DateTime LocalTime { get; }

        DateTime UtcTime { get; }
    }
}
