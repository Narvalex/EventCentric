using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Messaging
{
    public interface ICrudEventBus : IEventBus
    {
        void Publish<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
