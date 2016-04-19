using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public struct IncomingEventIsPoisoned : IMessage
    {
        public IncomingEventIsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            this.PoisonedEvent = poisonedEvent;
            this.Exception = exception;
        }

        public IEvent PoisonedEvent { get; }
        public PoisonMessageException Exception { get; }
    }
}
