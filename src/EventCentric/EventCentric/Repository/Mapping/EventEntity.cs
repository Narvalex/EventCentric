using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class EventEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public int Version { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public Guid? CorrelationId { get; set; }
        public int EventCollectionVersion { get; set; }
        public DateTime CreationDate { get; set; }
        public string Payload { get; set; }
    }

    public class EventEntityMap : EntityTypeConfiguration<EventEntity>
    {
        public EventEntityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.StreamType, t.StreamId, t.Version });

            // Properties
            this.Property(t => t.Version)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.StreamType)
                .HasMaxLength(255);

            this.Property(t => t.EventType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.EventCollectionVersion)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(t => t.Payload)
                .IsRequired();


            // Table & Column Mappings
            this.ToTable("Events", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.EventId).HasColumnName("EventId");
            this.Property(t => t.EventType).HasColumnName("EventType");
            this.Property(t => t.CorrelationId).HasColumnName("CorrelationId");
            this.Property(t => t.EventCollectionVersion).HasColumnName("EventCollectionVersion");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.Payload).HasColumnName("Payload");
        }
    }
}
