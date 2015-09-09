using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;

namespace EventCentric.Queueing
{
    public class EventQueue : FSM, IEventBus,
        IMessageHandler<StartEventQueue>,
        IMessageHandler<StopEventQueue>
    {
        protected readonly IQueueWriter writer;
        protected ConcurrentDictionary<Guid, object> streamLocksById;

        public EventQueue(IBus bus, ILogger log, IQueueWriter writer)
            : base(bus, log)
        {
            Ensure.NotNull(writer, "writer");

            this.writer = writer;
            this.streamLocksById = new ConcurrentDictionary<Guid, object>();
        }

        public void Send(IEvent @event)
        {
            this.streamLocksById.TryAdd(@event.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(@event.StreamId))
            {
                int version;
                try
                {
                    version = this.writer.Enqueue(@event);
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, "An errror ocurred in queue writer when writint event type {0}", @event.GetType().Name);
                    throw;
                }

#if DEBUG
                this.log.Trace("Event type {0} is now in queue", @event.GetType().Name);
#endif

                this.bus.Publish(new EventStoreHasBeenUpdated(version));
            }
        }

        public void Handle(StartEventQueue message)
        {
            base.log.Trace("Starting event queue");
            base.Start();
            base.log.Trace("Event queue started");
        }

        public void Handle(StopEventQueue message)
        {
            base.Stop();
        }

        protected override void OnStarting()
        {
            base.bus.Publish(new MessageQueueStarted());
        }

        protected override void OnStopping()
        {
            base.bus.Publish(new MessageQueueStopped());
        }
    }
}
