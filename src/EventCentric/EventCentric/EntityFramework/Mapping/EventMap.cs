using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.EntityFramework.Mapping
{
    public class EventMap : EntityTypeConfiguration<Event>
    {
        public EventMap()
        {
            // Primary Key
            this.HasKey(t => new { t.StreamId, t.Version });

            // Properties
            this.Property(t => t.Version)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.EventType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Payload)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Events", "EventStore");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.EventId).HasColumnName("EventId");
            this.Property(t => t.EventType).HasColumnName("EventType");
            this.Property(t => t.CorrelationId).HasColumnName("CorrelationId");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.Payload).HasColumnName("Payload");
        }
    }
}
