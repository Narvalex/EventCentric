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
    public class MessageQueue : FSM, IClientBus,
        IMessageHandler<StartMessageQueue>,
        IMessageHandler<StopMessageQueue>
    {
        private readonly IQueueWriter writer;
        protected ConcurrentDictionary<Guid, object> streamLocksById;

        public MessageQueue(IBus bus, IQueueWriter writer)
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

        private void Enqueue(IEvent message)
        {
            this.streamLocksById.TryAdd(message.StreamId, new object());
            lock (this.streamLocksById.TryGetValue(message.StreamId))
            {
                var updatedStreamVersion = this.writer.EnqueueMessage(message);
                this.bus.Publish(new StreamHasBeenUpdated(message.StreamId, updatedStreamVersion));
            }
        }

        public void Handle(StartMessageQueue message)
        {
            base.Start();
        }

        public void Handle(StopMessageQueue message)
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
