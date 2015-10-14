using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Utils;

namespace EventCentric.Queueing
{
    public class InMemoryQueueWriter<T> : Worker, IQueueWriter
    {
        protected static readonly string _streamType = $"{typeof(T).Name}_{typeof(T).GUID}";
        protected readonly IGuidProvider guid;

        public InMemoryQueueWriter(IGuidProvider guid, IBus bus)
             : base(bus)
        {
            Ensure.NotNull(guid, "guid");

            this.guid = guid;
        }

        public int Enqueue(IEvent @event)
        {
            ((Event)@event).StreamType = _streamType;
            ((Event)@event).EventId = this.guid.NewGuid();
            ((Event)@event).Version = 0;

            base.bus.Send(new NewIncomingEvents(new IEvent[] { @event }));
            return 0;
        }
    }
}
