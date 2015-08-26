using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class SubscriptionEntity
    {
        public string StreamType { get; set; }
        public string Url { get; set; }
        public int EventsInProcessorVersion { get; set; }
        public int EventCollectionVersion { get; set; }
        public bool IsPoisoned { get; set; }
        public string DeadLetterPayload { get; set; }
        public string ExceptionMessage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateTime { get; set; }
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

            this.Property(t => t.EventsInProcessorVersion)
               .IsRequired();

            this.Property(t => t.EventCollectionVersion)
                .IsRequired();

            this.Property(t => t.IsPoisoned)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Subscriptions", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.EventsInProcessorVersion).HasColumnName("EventsInProcessorVersion");
            this.Property(t => t.EventCollectionVersion).HasColumnName("EventCollectionVersion");
            this.Property(t => t.IsPoisoned).HasColumnName("IsPoisoned");
            this.Property(t => t.DeadLetterPayload).HasColumnName("DeadLetterPayload");
            this.Property(t => t.ExceptionMessage).HasColumnName("ExceptionMessage");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.UpdateTime).HasColumnName("UpdateTime");
        }
    }
}
