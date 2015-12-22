using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    /// <summary>
    /// Base class for event sourced aggregates that implements <see cref="IEventSourced"/> that contains 
    /// some common useful functionality related to versions and rehydration from past events.
    /// </summary>
    public abstract class EventSourced : IEventSourced, IMementoOriginator
    {
        private readonly Guid id;
        private long version = 0;
        private List<IEvent> pendingEvents = new List<IEvent>();

        protected EventSourced(Guid id)
        {
            this.id = id;
            this.OnUpdateStarted();
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

        public Guid Id => this.id;

        public long Version => this.version;

        public IEvent[] PendingEvents => this.pendingEvents.ToArray();

        /// <summary>
        /// Afther the aggregate id is set this method will be called.
        /// </summary>
        protected virtual void OnUpdateStarted() { }

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

        public virtual IMemento SaveToMemento() => new Memento(this.version);
    }
}
