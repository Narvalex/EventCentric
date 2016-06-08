using EventCentric.EventSourcing;
using System;

namespace EventCentric.Handling
{
    /// <summary>
    /// Represents a message handling function. 
    /// </summary>
    public interface IMessageHandling
    {
        Guid StreamId { get; }
        Func<IEventSourced> Handle { get; }
        bool DeduplicateBeforeHandling { get; }
    }
}
