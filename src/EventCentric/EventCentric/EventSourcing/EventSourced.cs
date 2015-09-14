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

        protected EventSourced(Guid id, IEnumerable<IEvent> streamOfEvents)
            : this(id)
        {
            foreach (var e in streamOfEvents)
                this.Apply(e);
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

        protected void Update(Event @event)
        {
            @event.StreamId = this.id;
            @event.Version = this.version + 1;
            this.Apply(@event);
            this.pendingEvents.Add(@event);
        }

        protected void Update(params Event[] events)
        {
            foreach (var @event in events)
                this.Update(@event);
        }

        private void Apply(IEvent @event)
        {
            ((dynamic)this).On((dynamic)@event);
            this.version = @event.Version;
        }

        public virtual IMemento SaveToMemento()
        {
            return new Memento(this.version);
        }
    }
}
