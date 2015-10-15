using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventIsPoisoned : IMessage
    {
        public IncomingEventIsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            this.PoisonedEvent = poisonedEvent;
            this.Exception = exception;
        }

        public IEvent PoisonedEvent { get; private set; }
        public PoisonMessageException Exception { get; private set; }
    }
}
