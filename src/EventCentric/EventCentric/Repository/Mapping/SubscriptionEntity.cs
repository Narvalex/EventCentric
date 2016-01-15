using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class SubscriptionEntity
    {
        public string StreamType { get; set; }
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

    public class SubscriptionEntityMap : EntityTypeConfiguration<SubscriptionEntity>
    {
        public SubscriptionEntityMap()
        {
            // Primary key
            this.HasKey(t => t.StreamType);

            // Properties
            this.Property(t => t.Url)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ProcessorBufferVersion)
               .IsRequired();

            this.Property(t => t.IsPoisoned)
                .IsRequired();

            this.Property(t => t.WasCanceled)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Subscriptions", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.Token).HasColumnName("Token");
            this.Property(t => t.ProcessorBufferVersion).HasColumnName("ProcessorBufferVersion");
            this.Property(t => t.IsPoisoned).HasColumnName("IsPoisoned");
            this.Property(t => t.WasCanceled).HasColumnName("WasCanceled");
            this.Property(t => t.PoisonEventCollectionVersion).HasColumnName("PoisonEventCollectionVersion");
            this.Property(t => t.DeadLetterPayload).HasColumnName("DeadLetterPayload");
            this.Property(t => t.ExceptionMessage).HasColumnName("ExceptionMessage");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
        }
    }
}
