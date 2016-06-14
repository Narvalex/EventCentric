using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence
{
    public class InboxEntityMap : EntityTypeConfiguration<InboxEntity>
    {
        public InboxEntityMap()
        {
            // Primary Key
            this.HasKey(t => t.EventId);

            // Properties
            this.Property(t => t.InboxStreamType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.StreamType)
                .IsOptional()
                .HasMaxLength(255);

            this.Property(t => t.EventType)
                .IsOptional()
                .HasMaxLength(255);

            this.Property(t => t.Payload)
                .IsOptional();

            this.Property(t => t.EventCollectionVersion)
                .IsOptional();

            this.Property(t => t.TransactionId)
                .IsOptional();

            // Table & Column Mappings
            this.ToTable("Inbox", "EventStore");
            this.Property(t => t.EventId).HasColumnName("EventId");
            this.Property(t => t.InboxStreamType).HasColumnName("InboxStreamType");
            this.Property(t => t.TransactionId).HasColumnName("TransactionId");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.EventType).HasColumnName("EventType");
            this.Property(t => t.EventCollectionVersion).HasColumnName("EventCollectionVersion");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.Payload).HasColumnName("Payload");
        }
    }
}
