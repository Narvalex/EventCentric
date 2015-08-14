using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;

namespace EventCentric.Processing
{
    public class EventProcessor<T> : Worker,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<NewIncomingEvent>
            where T : IEventSourced
    {
        private readonly ITextSerializer serializer;
        protected ConcurrentDictionary<Guid, object> streamLocksById;

        public EventProcessor(IBus bus, ITextSerializer serializer)
            : base(bus)
        {
            this.streamLocksById = new ConcurrentDictionary<Guid, object>();
        }

        public void Handle(NewIncomingEvent message)
        {
            ((dynamic)this).Handle((dynamic)message.Event);
        }

        public void Handle(StartEventProcessor message)
        {
            base.Start();
            base.bus.Publish(new EventProcessorStarted());
        }

        public void Handle(StopEventProcessor message)
        {
            base.Stop();
            this.bus.Publish(new EventProcessorStopped());
        }

        protected void CreateNewStreamAndHandle(Guid id, IEvent @event)
        {
            this.HandleWithStreamLocking(id, () =>
            {
                this
            });
        }

        protected void GetStreamAndHandle(Guid id, IEvent @event)
        {
            this.HandleWithStreamLocking(id, () =>
            {

            });
        }

        /// <summary>
        /// Mark as ignored in the inbox table and in the subscription table. 
        /// This node is not subscribed to this event, but is interested in other events that
        /// happened in the source.
        /// </summary>
        /// <param name="@event">The <see cref="IEvent"/> to be igonred.</param>
        protected void IgnoreEvent(IEvent @event)
        {
            // Mark in the db....
            this.bus.Publish(new IncomingEventHasBeenProcessed(@event.StreamId, @event.Version));
        }

        private void HandleWithStreamLocking(Guid id, Action handling)
        {
            this.streamLocksById.TryAdd(id, new object());
            lock (this.streamLocksById.TryGetValue(id))
            {
                handling();
            }
        }
    }
}
