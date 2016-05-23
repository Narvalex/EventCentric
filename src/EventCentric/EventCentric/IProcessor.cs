using EventCentric.EventSourcing;
using System;

namespace EventCentric
{
    /// <summary>
    /// Process incoing messages from an user, or an app service.
    /// </summary>
    public interface IProcessor
    {
        Guid Send(Guid streamId, Message message);
    }
}
