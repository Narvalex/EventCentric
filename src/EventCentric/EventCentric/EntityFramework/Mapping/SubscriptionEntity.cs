using System;
using System.Data.Entity.ModelConfiguration;

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

            this.Property(t => t.Url)
                .IsRequired()
                .HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Subscriptions", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.LastProcessedVersion).HasColumnName("LastProcessedVersion");
            this.Property(t => t.LasProcessedEventId).HasColumnName("LasProcessedEventId");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.IsPoisoned).HasColumnName("IsPoisoned");
            this.Property(t => t.ExceptionMessage).HasColumnName("ExceptionMessage");
        }
    }
}
