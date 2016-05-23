using System;

namespace EventCentric.EventSourcing
{
    public interface IEventStore
    {
        string StreamName { get; }

        long CurrentEventCollectionVersion { get; }

        SerializedEvent[] FindEvents(long fromEventCollectionVersion, int quantity);
    }

    public interface IEventStore<T> : IEventStore where T : IEventSourced
    {
        /// <summary>
        /// Tries to retrieve the event sourced aggregate.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <returns>The hydrated entity, or null if it does not exist.</returns>
        T Find(Guid id);

        /// <summary>
        /// Retrieves the event sourced aggregate.
        /// </summary>
        /// <param name="id">The id of the aggregate.</param>
        /// <returns>The hydrated aggregate.</returns>
        /// <exception cref="StreamNotFoundException">If the stream is not found.</exception>
        T Get(Guid id);

        /// <summary>
        /// Saves the event sourced aggregate.
        /// </summary>
        /// <param name="eventSourced">The event sourced aggregate.</param>
        /// <param name="incomingEvent">The correlated <see cref="IEvent"/></param>
        /// <returns>The event collection version.</returns>
        void Save(T eventSourced, IEvent incomingEvent);

        bool IsDuplicate(Guid eventId);

        void DeleteSnapshot(Guid streamId);
    }
}
