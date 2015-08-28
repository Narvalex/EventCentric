using EventCentric.EventSourcing;
using EventCentric.Transport;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventIsPoisoned : IMessage
    {
        public IncomingEventIsPoisoned(IncomingEvent<IEvent> poisonedEvent, PoisonMessageException exception)
        {
            this.PoisonedEvent = poisonedEvent;
            this.Exception = exception;
        }

        public IncomingEvent<IEvent> PoisonedEvent { get; private set; }
        public PoisonMessageException Exception { get; private set; }
    }
}
