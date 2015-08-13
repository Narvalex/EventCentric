using System;

namespace EventCentric.EventSourcing
{
    public interface IEvent
    {
        string StreamType { get; }
        Guid StreamId { get; }
    }
}
