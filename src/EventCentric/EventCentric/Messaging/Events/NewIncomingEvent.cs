using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvent : IMessage
    {
        public NewIncomingEvent(IEvent @event)
        {
            this.Event = @event;
        }

        public IEvent Event { get; private set; }

    }
}
