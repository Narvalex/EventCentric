using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Queueing
{
    public interface ICrudEventBus : IEventBus
    {
        void Send<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
