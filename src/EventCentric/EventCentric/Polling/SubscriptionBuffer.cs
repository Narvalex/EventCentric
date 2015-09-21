using EventCentric.Transport;
using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public class SubscriptionBuffer : IMonitoredSubscription
    {
        public SubscriptionBuffer(string streamType, string url, string token, int currentBufferVersion, bool isPoisoned)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.Token = token;
            this.CurrentBufferVersion = currentBufferVersion;
            this.IsPolling = false;
            this.IsPoisoned = isPoisoned;
            this.NewEventsQueue = new ConcurrentQueue<NewRawEvent>();
            this.EventsInProcessorBag = new ConcurrentBag<EventInProcessorBucket>();
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }
        public string Token { get; private set; }

        /// <summary>
        /// The processor buffer version is the lowest event collection version that the processor was handling when starting from cold.
        /// When is on running, is the lastest buffer version that was polled.
        /// </summary>
        public volatile int CurrentBufferVersion;
        public volatile bool IsPolling;
        public volatile bool IsPoisoned;

        /// <summary>
        /// A queue where the events are being reserved
        /// </summary>
        public volatile ConcurrentQueue<NewRawEvent> NewEventsQueue;

        /// <summary>
        /// A bag that monitors the events that are currently being processed
        /// </summary>
        public volatile ConcurrentBag<EventInProcessorBucket> EventsInProcessorBag;

        // Metrics;
        internal volatile int consumerVersion;
        internal volatile int producerVersion;

        public int ConsumerVersion
        {
            get { return this.consumerVersion; }
        }

        public int ProducerVersion
        {
            get { return this.producerVersion; }
        }

        public decimal UpToDatePercentage
        {
            get { return (this.consumerVersion * 100) / this.producerVersion; }
        }

        public string ProducerName
        {
            get { return this.StreamType; }
        }
    }
}
