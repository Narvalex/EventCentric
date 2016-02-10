using EventCentric.EventSourcing;
using System;

namespace EventCentric.Messaging
{
    public interface IServiceBus
    {
        void Send(Guid transactionId, Guid streamId, Message message);
    }
}
