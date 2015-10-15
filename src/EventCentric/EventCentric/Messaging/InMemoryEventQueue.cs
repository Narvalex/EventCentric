using EventCentric.EventSourcing;
using EventCentric.Messaging.Events;
using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public class InMemoryEventQueue : Worker, IEventQueue
    {
        protected readonly string appName;
        protected readonly IGuidProvider guid;

        public InMemoryEventQueue(string appName, IGuidProvider guid, IBus bus)
             : base(bus)
        {
            Ensure.NotNullEmtpyOrWhiteSpace(appName, "appName");
            Ensure.NotNull(guid, "guid");

            this.appName = appName;
            this.guid = guid;
        }

        public void Enqueue(IEvent @event)
        {
            ((Event)@event).StreamType = this.appName;
            ((Event)@event).EventId = this.guid.NewGuid();
            ((Event)@event).Version = 0;

            base.bus.Send(new NewIncomingEvent(@event));
        }
    }
}
