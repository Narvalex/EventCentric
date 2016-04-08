using EventCentric.Microservice;
using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    /// <summary>
    /// Represents an identifiable aggregate that is event sourced.
    /// </summary>
    public interface IEventSourced : INamedEventSource
    {
        /// <summary>
        /// Gets the aggregate identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the aggregate's version. As the aggregate is being updated and events being generated, the version is incremented. 
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the collection of new events since the aggregate was hydrated, as a consequence of message handling.
        /// </summary>
        IList<IEvent> PendingEvents { get; }
    }
}
