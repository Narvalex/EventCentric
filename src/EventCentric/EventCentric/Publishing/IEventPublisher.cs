using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Ther source name, also known as StreamType
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Poll events from the publisher.
        /// </summary>
        /// <param name="eventBufferVersion">The last event version that the consumer had in buffer. 
        /// this means that the consumer is asking for events that are GREATER than that version.</param>
        /// <param name="consumerName">The consumer's name.</param>
        /// <returns></returns>
        PollResponse PollEvents(long eventBufferVersion, string consumerName);
    }
}
