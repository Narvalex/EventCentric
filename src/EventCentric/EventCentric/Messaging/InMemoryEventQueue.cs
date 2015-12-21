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
        private readonly IUtcTimeProvider time;

        public InMemoryEventQueue(string streamType, IGuidProvider guid, IBus bus, IUtcTimeProvider time)
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
            var now = this.time.Now;
            base.bus.Send(new NewIncomingEvent(@event.AsQueuedEvent(this.streamType, this.guid.NewGuid(), InMemoryVersioning.GetNextVersion(), now, now.ToLocalTime())));
        }
    }
}
