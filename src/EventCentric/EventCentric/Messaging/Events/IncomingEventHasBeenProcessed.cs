using EventCentric.EventSourcing;

namespace EventCentric.Messaging.Events
{
    public struct IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(IEvent e)
        {
            this.Event = e;
        }

        public IEvent Event { get; }
    }
}
