using EventCentric.EventSourcing;
using System.Collections.Generic;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvents : IMessage
    {
        public NewIncomingEvents(IEnumerable<IEvent> incomingEvents)
        {
            this.IncomingEvents = incomingEvents;
        }

        public IEnumerable<IEvent> IncomingEvents { get; }
    }
}
