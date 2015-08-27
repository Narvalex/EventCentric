using EventCentric.EventSourcing;
using EventCentric.Transport;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvent : IMessage
    {
        public NewIncomingEvent(IncomingEvent<IEvent> incomingEvent)
        {
            this.IncomingEvent = incomingEvent;
        }

        public IncomingEvent<IEvent> IncomingEvent { get; private set; }
    }
}
