namespace EventCentric.Config
{
    public class HardcodedPollerConfig : IPollerConfig
    {
        public HardcodedPollerConfig(int bufferQueueMaxCount, int eventsToFlushMaxCount, double timeout)
        {
            this.BufferQueueMaxCount = bufferQueueMaxCount;
            this.EventsToFlushMaxCount = eventsToFlushMaxCount;
            this.Timeout = timeout;
        }

        public HardcodedPollerConfig()
            : this(
                  1000,
                  100,
                  90000) // we wait 30 seconds more than the publisher timeout
        { }

        public int BufferQueueMaxCount { get; }

        public int EventsToFlushMaxCount { get; }

        public double Timeout { get; }
    }
}
