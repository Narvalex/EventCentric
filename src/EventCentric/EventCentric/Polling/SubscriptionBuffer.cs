using EventCentric.Transport;
using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public class SubscriptionBuffer : IMonitoredSubscription
    {
        public SubscriptionBuffer(string streamType, string url, string token, long currentBufferVersion, bool isPoisoned)
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

        public string StreamType { get; }
        public string Url { get; }
        public string Token { get; }

        /// <summary>
        /// The processor buffer version is the lowest event collection version that the processor was handling when starting from cold.
        /// When is on running, is the lastest buffer version that was polled.
        /// </summary>
        public long CurrentBufferVersion { get; set; }

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
        public long ConsumerVersion { get; internal set; }

        public long ProducerVersion { get; internal set; }

        public decimal UpToDatePercentage =>
            this.ProducerVersion == 0 ? 100
                                      : (this.ConsumerVersion * 100) / this.ProducerVersion;


        public string ProducerName => this.StreamType;
    }
}
