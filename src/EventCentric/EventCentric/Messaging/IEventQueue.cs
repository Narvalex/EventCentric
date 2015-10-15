using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Messaging
{
    public interface IEventQueue
    {
        /// <summary>
        /// Enqueue a message in the queue
        /// </summary>
        /// <param name="event">The message to enqueue.</param>
        /// <returns>The event stream collection version.</returns>
        void Enqueue(IEvent @event);
    }

    public interface ICrudEventQueue : IEventQueue
    {
        void Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
