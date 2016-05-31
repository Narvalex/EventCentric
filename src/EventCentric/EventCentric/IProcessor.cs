using EventCentric.EventSourcing;
using System;

namespace EventCentric
{
    /// <summary>
    /// Process incoing messages from an user, or an app service.
    /// </summary>
    public interface IProcessor<T> where T : class, IEventSourced
    {
        /// <summary>
        /// Sends a message to the event processor. If the message has a defined EventId and transaction Id, 
        /// it will be idempotent.
        /// </summary>
        /// <param name="streamId">The stream id that the message belongs to.</param>
        /// <param name="message">The message to be processed. It could be an event or a command.</param>
        /// <returns>The transaction identifier, useful to poll the read side.</returns>
        Guid Process(Message message);

        /// <summary>
        /// Process a message as soon as possible and returns the state of <see cref="T"/>. If the message has a 
        /// defined EventId and transaction Id, it will be idempotent.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The upadted state of <see cref="T"/></returns>
        T ProcessNow(Message message);
    }
}
