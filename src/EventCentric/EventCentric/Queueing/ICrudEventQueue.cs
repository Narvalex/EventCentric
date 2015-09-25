using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Queueing
{
    public interface ICrudEventQueue : IEventQueue
    {
        void Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext;
    }
}
