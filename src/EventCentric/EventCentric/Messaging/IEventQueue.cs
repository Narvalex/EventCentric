using EventCentric.EventSourcing;

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
}
