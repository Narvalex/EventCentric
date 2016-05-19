using System;

namespace EventCentric.Persistence
{
    // About unique constraints vs indexes: https://technet.microsoft.com/en-us/library/aa224827(v=sql.80).aspx
    public class EventEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public long Version { get; set; }
        public Guid TransactionId { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public Guid? CorrelationId { get; set; }
        public long EventCollectionVersion { get; set; }
        public DateTime LocalTime { get; set; }
        public DateTime UtcTime { get; set; }
        public byte[] RowVersion { get; set; }
        public string Payload { get; set; }
    }

    public class InboxEntity
    {
        public long InboxId { get; set; }
        public string InboxStreamType { get; set; }
        public Guid EventId { get; set; }
        public Guid? TransactionId { get; set; }
        public string StreamType { get; set; }
        public Guid? StreamId { get; set; }
        public long? Version { get; set; }
        public string EventType { get; set; }
        public long? EventCollectionVersion { get; set; }
        public DateTime CreationLocalTime { get; set; }
        public string Payload { get; set; }
    }

    public partial class SnapshotEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public long Version { get; set; }
        public string Payload { get; set; }
        public DateTime CreationLocalTime { get; set; }
        public DateTime UpdateLocalTime { get; set; }
    }

    public class SubscriptionEntity
    {
        /// <summary>
        /// The subscriber's stream unique name
        /// </summary>
        public string SubscriberStreamType { get; set; }

        /// <summary>
        /// The producer's stream name
        /// </summary>
        public string StreamType { get; set; }

        /// <summary>
        /// The url of the producer. If the producer shares memory with the consumer, 
        /// then the url should be "none"
        /// </summary>
        public string Url { get; set; }
        public string Token { get; set; }
        public long ProcessorBufferVersion { get; set; }
        public bool IsPoisoned { get; set; }
        public bool WasCanceled { get; set; }

        /// <summary>
        /// The event collection version of the poisoned message.
        /// </summary>
        public long? PoisonEventCollectionVersion { get; set; }
        public string DeadLetterPayload { get; set; }
        public string ExceptionMessage { get; set; }
        public DateTime CreationLocalTime { get; set; }
        public DateTime UpdateLocalTime { get; set; }
    }
}
