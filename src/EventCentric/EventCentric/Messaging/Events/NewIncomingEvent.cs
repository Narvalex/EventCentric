using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvent : IMessage
    {
        public NewIncomingEvent(IEvent incomingEvent)
        {
            this.IncomingEvent = incomingEvent;
        }

        public IEvent IncomingEvent { get; private set; }
    }
}
