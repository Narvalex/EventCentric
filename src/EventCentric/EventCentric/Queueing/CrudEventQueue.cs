using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Utils;
using System;

namespace EventCentric.Queueing
{
    public class CrudEventQueue : EventQueue, ICrudEventBus
    {
        public CrudEventQueue(IBus bus, ILogger log, ICrudQueueWriter writer)
            : base(bus, log, writer)
        { }

        public void Send<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext
        {
            base.streamLocksById.TryAdd(@event.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(@event.StreamId))
            {
                var version = ((ICrudQueueWriter)this.writer).Enqueue(@event, performCrudOperation);
                this.bus.Publish(new EventStoreHasBeenUpdated(version));
            }
        }
    }
}
