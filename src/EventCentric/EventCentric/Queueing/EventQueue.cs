using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Processing;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;

namespace EventCentric.Queueing
{
    public class EventQueue : FSM, IClientBus,
        IMessageHandler<StartEventQueue>,
        IMessageHandler<StopEventQueue>
    {
        private readonly IQueueWriter writer;
        protected ConcurrentDictionary<Guid, object> streamLocksById;

        public EventQueue(IBus bus, IQueueWriter writer)
            : base(bus)
        {
            Ensure.NotNull(writer, "writer");

            this.writer = writer;
            this.streamLocksById = new ConcurrentDictionary<Guid, object>();
        }

        public void Send(ICommand command)
        {
            this.Enqueue(command);
        }

        public void Publish(IEvent @event)
        {
            this.Enqueue(@event);
        }

        private void Enqueue(IEvent @event)
        {
            this.streamLocksById.TryAdd(@event.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(@event.StreamId))
            {
                var updatedStreamVersion = this.writer.Enqueue(@event);
                this.bus.Publish(new StreamHasBeenUpdated(@event.StreamId, updatedStreamVersion));
            }
        }

        public void Handle(StartEventQueue message)
        {
            base.Start();
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
