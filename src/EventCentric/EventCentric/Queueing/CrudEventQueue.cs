using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Utils;
using System;

namespace EventCentric.Queueing
{
    public class CrudEventQueue : EventQueue, ICrudEventQueue
    {
        public CrudEventQueue(IBus bus, ILogger log, ICrudQueueWriter writer)
            : base(bus, log, writer)
        { }

        public void Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext
        {
            base.streamLocksById.TryAdd(@event.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(@event.StreamId))
            {
                int version;
                try
                {
                    version = ((ICrudQueueWriter)this.writer).Enqueue(@event, performCrudOperation);
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, "An errror ocurred in crud-queue writer when writing event type {0}", @event.GetType().Name);
                    throw;
                }

#if DEBUG
                this.log.Trace("Event type {0} is now in queue", @event.GetType().Name);
#endif

                this.bus.Publish(new EventStoreHasBeenUpdated(version));
            }
        }
    }
}
