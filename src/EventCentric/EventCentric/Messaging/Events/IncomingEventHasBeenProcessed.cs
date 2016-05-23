using EventCentric.EventSourcing;
using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public IncomingEventHasBeenProcessed(IEvent e)
        {
            this.Event = e;
        }

        public IEvent Event { get; }
    }
}
