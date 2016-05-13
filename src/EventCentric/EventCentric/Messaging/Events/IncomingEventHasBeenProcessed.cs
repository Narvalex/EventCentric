using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public struct IncomingEventsHasBeenProcessed : IMessage
    {
        public IncomingEventsHasBeenProcessed(IEvent[] events)
        {
            this.Events = events;
        }

        public IEvent[] Events { get; }
    }
}
