using EventCentric.Processing;
using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    public abstract class EventSourced : IEventSourced, IMementoOriginator
    {
        private readonly Guid id;
        private int version = 0;
        private List<IEvent> pendingEvents = new List<IEvent>();

        protected EventSourced(Guid id)
        {
            this.id = id;
        }

        protected EventSourced(Guid id, IMemento memento)
            : this(id)
        {
            this.version = memento.Version;
        }

        public Guid Id
        {
            get { return this.id; }
        }

        public int Version
        {
            get { return this.version; }
        }

        public IEvent[] PendingEvents
        {
            get { return this.pendingEvents.ToArray(); }
        }

        protected void Publish(Event @event)
        {
            @event.StreamId = this.id;
            @event.Version = this.version + 1;
            ((dynamic)this).On((dynamic)@event);
            this.version = @event.Version;
            this.pendingEvents.Add(@event);
        }

        protected void Publish(params Event[] events)
        {
            foreach (var @event in events)
                this.Publish(@event);
        }

        protected void Send(Command command)
        {
            this.Publish(command);
        }

        protected void Send(params Command[] commands)
        {
            this.Publish(commands);
        }

        public virtual IMemento SaveToMemento()
        {
            return new Memento { Version = this.version };
        }

        internal class Memento : IMemento
        {
            public int Version { get; internal set; }
        }
    }
}
