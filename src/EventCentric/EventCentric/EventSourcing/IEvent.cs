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
        int Version { get; }

        int EventCollectionVersion { get; }

        int ProcessorBufferVersion { get; }
    }
}
