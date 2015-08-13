using System;

namespace EventCentric.EntityFramework
{
    public partial class SubscriptionEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public string Url { get; set; }
        public int LastProcessedVersion { get; set; }
        public Guid LasProcessedEventId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsPoisoned { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
