using System;

namespace EventCentric.EventSourcing
{
    public interface IEvent
    {
        string StreamType { get; }
        Guid StreamId { get; }

        /// <summary>
        /// The version of the stream when the event happened.
        /// </summary>
        int Version { get; }
    }
}
