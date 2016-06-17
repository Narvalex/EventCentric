namespace EventCentric.Config
{
    public class HardcodedPollerConfig : IPollerConfig
    {
        /// <summary>
        /// HardcodedPollerConfig constructor.
        /// </summary>
        /// <param name="bufferQueueMaxCount"></param>
        /// <param name="eventsToFlushMaxCount"></param>
        /// <param name="timeout">We wait MORE than the publisher timeout, by default.</param>
        public HardcodedPollerConfig(int bufferQueueMaxCount = 2000, int eventsToFlushMaxCount = 100, double timeout = 180000)
        {
            this.BufferQueueMaxCount = bufferQueueMaxCount;
            this.EventsToFlushMaxCount = eventsToFlushMaxCount;
            this.Timeout = timeout;
        }

        public int BufferQueueMaxCount { get; }

        public int EventsToFlushMaxCount { get; }

        public double Timeout { get; }
    }
}
