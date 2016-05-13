using EventCentric.EventSourcing;
using System.Collections.Generic;

namespace EventCentric.Messaging.Events
{
    public struct NewIncomingEvents : IMessage
    {
        public NewIncomingEvents(IEnumerable<IEvent> incomingEvents)
        {
            this.IncomingEvents = incomingEvents;
        }

        /// <summary>
        /// The events are just ordered by event collection version. Concurrency should be handled by the handler
        /// </summary>
        public IEnumerable<IEvent> IncomingEvents { get; }
    }
}
