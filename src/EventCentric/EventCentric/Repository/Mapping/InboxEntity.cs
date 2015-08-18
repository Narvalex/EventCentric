using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class InboxEntity
    {
        public long InboxId { get; set; }
        public Guid EventId { get; set; }
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public int Version { get; set; }
        public string EventType { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Ignored { get; set; }
        public string Payload { get; set; }
    }

    public class InboxEntityMap : EntityTypeConfiguration<InboxEntity>
    {
        public InboxEntityMap()
        {
            // Primary Key
            this.HasKey(t => t.InboxId);

            // Properties
            this.Property(t => t.StreamType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.EventType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Payload)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Inbox", "EventStore");
            this.Property(t => t.InboxId).HasColumnName("InboxId");
            this.Property(t => t.EventId).HasColumnName("EventId");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.EventType).HasColumnName("EventType");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.Ignored).HasColumnName("Ignored");
            this.Property(t => t.Payload).HasColumnName("Payload");
        }
    }
}
