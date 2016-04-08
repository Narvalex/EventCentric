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
        private long version = 0;
        private List<IEvent> pendingEvents = new List<IEvent>();

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

        protected T UpdateFromMessage(Message @event)
        {
            @event.StreamId = this.id;
            @event.Version = this.version + 1;
            this.Apply(@event);
            this.pendingEvents.Add(@event);
            return this as T;
        }

        private void Apply(IEvent @event)
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
