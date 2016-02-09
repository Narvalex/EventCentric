using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    public interface IEventHandling
    {
        Guid StreamId { get; }
        bool ShouldBeIgnored { get; }
        Func<IEventSourced> Handle { get; }
    }
}
