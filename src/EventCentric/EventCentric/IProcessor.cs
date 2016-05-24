using EventCentric.EventSourcing;
using System;

namespace EventCentric
{
    /// <summary>
    /// Process incoing messages from an user, or an app service.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Sends a message to the event processor.
        /// </summary>
        /// <param name="streamId">The stream id that the messafe belongs to.</param>
        /// <param name="message">The message to be processed. It could be an event or a command.</param>
        /// <returns></returns>
        Guid Send(Guid streamId, Message message);
    }
}
