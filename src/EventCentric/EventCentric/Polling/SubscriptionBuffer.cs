using EventCentric.Transport;
using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public class SubscriptionBuffer : IMonitoredSubscription
    {
        private long currentBufferVersion;
        private long consumerVersion;
        private long producerVersion;

        // Locks
        private static object _currentBufferVersionLock = new object();

        private static object _consumerVersionLock = new object();
        private static object _producerVersionLock = new object();


        public SubscriptionBuffer(string streamType, string url, string token, long currentBufferVersion, bool isPoisoned)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.Token = token;
            this.currentBufferVersion = currentBufferVersion;
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
        public long CurrentBufferVersion
        {
            get
            {
                lock (_currentBufferVersionLock)
                {
                    return this.currentBufferVersion;
                }
            }
            set
            {
                lock (_currentBufferVersionLock)
                {
                    this.currentBufferVersion = value;
                }
            }
        }

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

        // Metrics
        public long ConsumerVersion
        {
            get
            {
                lock (_consumerVersionLock)
                {
                    return this.consumerVersion;
                }
            }
            set
            {
                lock (_consumerVersionLock)
                {
                    this.consumerVersion = value;
                }
            }
        }

        public long ProducerVersion
        {
            get
            {
                lock (_producerVersionLock)
                {
                    return this.producerVersion;
                }
            }
            set
            {
                lock (_producerVersionLock)
                {
                    this.producerVersion = value;
                }
            }
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
