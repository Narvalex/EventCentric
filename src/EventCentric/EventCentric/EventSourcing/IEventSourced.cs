using System;

namespace EventCentric.EventSourcing
{
    public interface IEventSourced
    {
        Guid Id { get; }
        int Version { get; }
    }
}
