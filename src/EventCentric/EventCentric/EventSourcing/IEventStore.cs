using System;

namespace EventCentric.EventSourcing
{
    public interface IEventStore<T> where T : IEventSourced
    {
        /// <summary>
        /// Retrieves the event sourced entity.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns>The hydrated entity</returns>
        /// <exception cref="StreamNotFoundException">If the stream is not found.</exception>
        T Get(Guid id);

        /// <summary>
        /// Saves the event sourced entity.
        /// </summary>
        /// <param name="eventSourced">The event sourced entity.</param>
        /// <param name="correlatedEvent">The correlated <see cref="IEvent"/></param>
        /// <returns>The stream collection version.</returns>
        int Save(T eventSourced, IEvent correlatedEvent);

        int Denormalize(T eventSourced, IEvent correlatedEvent);
    }
}
