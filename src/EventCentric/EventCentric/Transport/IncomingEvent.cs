using EventCentric.EventSourcing;

namespace EventCentric.Transport
{
    public class IncomingEvent<T> where T : IEvent
    {
        public IncomingEvent(int eventCollectionVersion, int processorBufferVersion, T @event)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.ProcessorBufferVersion = processorBufferVersion;
            this.Event = @event;
        }

        public int EventCollectionVersion { get; private set; }
        public int ProcessorBufferVersion { get; private set; }
        public T Event { get; private set; }
    }
}
