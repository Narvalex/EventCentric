using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Repository;
using System;
using System.Collections.Generic;

namespace EventCentric.Messaging
{
    public class CrudEventBus : EventBus, ICrudEventBus
    {
        public CrudEventBus(IBus bus, ILogger log, ICrudEventQueue queue)
            : base(bus, log, queue)
        { }

        public void Publish<T>(Guid transactionId, Guid streamId, IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext
        {
            base.streamLocksById.TryAdd(@event.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(@event.StreamId))
            {
                try
                {
                    ((ICrudEventQueue)this.queue).Enqueue(@event.AsIncomingMessage(transactionId, streamId), performCrudOperation);
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, "An errror ocurred in crud-queue writer when writing event type {0}", @event.GetType().Name);
                    throw;
                }

#if DEBUG
                this.log.Trace("Event type {0} is now in queue", @event.GetType().Name);
#endif
            }
        }
    }
}
