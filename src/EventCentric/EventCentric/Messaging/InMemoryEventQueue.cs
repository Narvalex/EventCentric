using EventCentric.EventSourcing;
using EventCentric.Messaging.Events;
using EventCentric.Messaging.Infrastructure;
using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public class InMemoryEventQueue : Worker, IEventQueue
    {
        private readonly string streamType;
        private readonly IGuidProvider guid;
        private readonly ITimeProvider time;

        public InMemoryEventQueue(string streamType, IGuidProvider guid, IBus bus, ITimeProvider time)
             : base(bus)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, "streamType");
            Ensure.NotNull(guid, "guid");
            Ensure.NotNull(time, "time");

            this.streamType = streamType;
            this.guid = guid;
            this.time = time;
        }

        public void Enqueue(IEvent @event)
        {
            base.bus.Send(new NewIncomingEvent(@event.AsQueuedEvent(this.streamType, this.guid.NewGuid(), InMemoryVersioning.GetNextVersion(), this.time.Now)));
        }
    }
}
