using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Queueing
{
    public interface IQueueWriter
    {
        /// <summary>
        /// Enqueue a message in the queue
        /// </summary>
        /// <param name="event">The message to enqueue.</param>
        /// <returns>The event stream collection version.</returns>
        int Enqueue(IEvent @event);
    }

    public interface ICrudQueueWriter : IQueueWriter
    {
        int Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
