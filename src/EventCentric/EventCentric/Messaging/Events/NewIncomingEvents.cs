using EventCentric.EventSourcing;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class NewIncomingEvents : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

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
