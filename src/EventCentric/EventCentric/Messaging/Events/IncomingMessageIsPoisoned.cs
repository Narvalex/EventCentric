using EventCentric.EventSourcing;
using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventIsPoisoned : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public IncomingEventIsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            this.PoisonedEvent = poisonedEvent;
            this.Exception = exception;
        }

        public IEvent PoisonedEvent { get; }
        public PoisonMessageException Exception { get; }
    }
}
