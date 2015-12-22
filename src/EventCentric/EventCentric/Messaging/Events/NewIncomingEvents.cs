using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvents : IMessage
    {
        public NewIncomingEvents(IEvent[] incomingEvents)
        {
            this.IncomingEvents = incomingEvents;
        }

        public IEvent[] IncomingEvents { get; private set; }
    }
}
