using EventCentric.EventSourcing;
using System;

namespace EventCentric.Messaging
{
    public interface IEventBus
    {
        void Publish(Guid transactionId, Guid streamId, IEvent @event);
    }
}
