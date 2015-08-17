using System;

namespace EventCentric.EventSourcing
{
    public interface IEventSourced
    {
        /// <summary>
        /// Gets the aggregates identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Get the stream version from which the aggregates sources from.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the collection of new events since the aggregate was hidrated, as a
        /// consequence of messages handling.
        /// </summary>
        IEvent[] PendingEvents { get; }
    }
}
