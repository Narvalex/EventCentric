using System;

namespace EventCentric.EventSourcing
{
    public interface IEvent
    {
        /// <summary>
        /// The id of the event.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// The version of the stream when the event happened.
        /// </summary>
        int Version { get; }

        string StreamType { get; }

        Guid StreamId { get; }


    }
}
