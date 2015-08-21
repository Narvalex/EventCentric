using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository
{
    public partial class SubscriptionEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public int LastProcessedVersion { get; set; }
        public Guid LastProcessedEventId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsPoisoned { get; set; }
        public string ExceptionMessage { get; set; }
    }

    public class SubscriptionEntityMap : EntityTypeConfiguration<SubscriptionEntity>
    {
        public SubscriptionEntityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.StreamType, t.StreamId });

            // Properties
            this.Property(t => t.StreamType)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Subscriptions", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.LastProcessedVersion).HasColumnName("LastProcessedVersion");
            this.Property(t => t.LastProcessedEventId).HasColumnName("LastProcessedEventId");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.IsPoisoned).HasColumnName("IsPoisoned");
            this.Property(t => t.ExceptionMessage).HasColumnName("ExceptionMessage");
        }
    }
}
