using System;
using System.Collections.Generic;

namespace EventCentric.EventSourcing
{
    /// <summary>
    /// Base class for event sourced aggregates that implements <see cref="IEventSourced"/> that contains 
    /// some common useful functionality related to versions and rehydration from past events.
    /// </summary>
    public abstract class EventSourced<TAggregate> : IEventSourced, ISnapshotOriginator
        where TAggregate : class, IEventSourced
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

        public IEvent[] PendingEvents => this.pendingEvents.ToArray();

        public TAggregate Update(Event @event) => UpdateFromMessage(@event);

        public TAggregate UpdateAfterSending(Command command) => UpdateFromMessage(command);

        public TAggregate Throw(string message) => this.Update(new AnInvalidOperationExceptionOccurred(message));

        private TAggregate UpdateFromMessage(Message @event)
        {
            @event.StreamId = this.id;
            @event.Version = this.version + 1;
            this.Apply(@event);
            this.pendingEvents.Add(@event);
            return this as TAggregate;
        }

        private void Apply(IEvent @event)
        {
            if (@event.IsACommand)
                ((dynamic)this).AfterSending((dynamic)@event);
            else
                ((dynamic)this).When((dynamic)@event);

            this.version = @event.Version;
        }

        public virtual ISnapshot SaveToSnapshot() => new Snapshot(this.version);
    }
}
