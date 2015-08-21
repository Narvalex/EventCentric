using EventCentric.EventSourcing;
using System;

namespace EventCentric.Queueing
{
    public interface IQueueWriter
    {
        /// <summary>
        /// Enqueue a message in the queue
        /// </summary>
        /// <param name="event">The message to enqueue.</param>
        /// <returns>The updated stream version in the queue and the updated stream collection version.</returns>
        Tuple<int, int> Enqueue(IEvent @event);
    }
}
