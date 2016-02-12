using EventCentric.EventSourcing;

namespace EventCentric.Transport
{
    public class EventInProcessorBucket
    {
        public EventInProcessorBucket(IEvent @event)
        {
            this.Event = @event;
            this.WasProcessed = false;
        }

        public IEvent Event { get; private set; }

        public bool WasProcessed { get; private set; }

        public void MarkEventAsProcessed()
        {
            this.WasProcessed = true;
        }
    }
}
