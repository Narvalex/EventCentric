using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.EntityFramework.Mapping
{
    public class InboxMap : EntityTypeConfiguration<Inbox>
    {
        public InboxMap()
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
            this.Property(t => t.Payload).HasColumnName("Payload");
        }
    }
}
