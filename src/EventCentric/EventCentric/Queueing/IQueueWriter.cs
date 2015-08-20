using EventCentric.EventSourcing;

namespace EventCentric.Queueing
{
    public interface IQueueWriter
    {
        /// <summary>
        /// Enqueue a message in the queue
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <returns>The updated stream version in the queue.</returns>
        int EnqueueMessage(IEvent message);
    }
}
