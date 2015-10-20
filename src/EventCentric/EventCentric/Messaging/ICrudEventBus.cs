using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Messaging
{
    public interface ICrudEventBus : IEventBus
    {
        void Publish<T>(Guid transactionId, Guid streamId, IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
