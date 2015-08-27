namespace EventCentric.Polling
{
    public class Subscription
    {
        public Subscription(string streamType, string url, int processorBufferVersion)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.ProcessorBufferVersion = processorBufferVersion;
            this.IsPolling = false;
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }

        /// <summary>
        /// The processor buffer version is the lowest event collection version that the processor was handling.
        /// </summary>
        public volatile int ProcessorBufferVersion;
        public volatile bool IsPolling;
    }
}
