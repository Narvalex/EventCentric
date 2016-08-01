using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    /// <summary>
    /// Base class for event sourced aggregates that implements <see cref="IEventSourced"/> that contains 
    /// some common useful functionality related to versions and rehydration from past events.
    /// </summary>
    public abstract class EventSourced<T> : IEventSourced, ISnapshotOriginator
        where T : class, IEventSourced
    {
        private readonly Guid id;
        private long version = -1;
        private List<IEvent> pendingEvents = new List<IEvent>();

        protected EventSourced()  { }

        public EventSourced(Guid id)
        {
            this.id = id;
        }

        public EventSourced(Guid id, IEnumerable<IEvent> streamOfEvents)
            : this(id)
        {
            foreach (var e in streamOfEvents)
                this.Apply(e);
        }

        public EventSourced(Guid id, ISnapshot snapshot)
            : this(id)
        {
            this.version = snapshot.Version;
        }

        public Guid Id => this.id;

        public long Version => this.version;

        public IList<IEvent> PendingEvents => this.pendingEvents;

        protected virtual T UpdateFromMessage(Message @event)
        {
            var now = DateTime.UtcNow;
            @event.StreamId = this.id;
            @event.Version = this.version + 1;
            @event.UtcTime = now;
            @event.LocalTime = now.ToLocalTime();

            this.Apply(@event);
            this.pendingEvents.Add(@event);
            return this as T;
        }

        protected void Apply(IEvent @event)
        {
            dynamic me = this;
            if (!@event.IsACommand)
                me.When((dynamic)@event);
            else
                me.AfterSending((dynamic)@event);

            this.version = @event.Version;
        }

        public virtual ISnapshot SaveToSnapshot() => new Snapshot(this.version);
    }
}
