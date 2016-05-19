using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence
{
    public class InboxEntityMapCe : EntityTypeConfiguration<InboxEntity>
    {
        public InboxEntityMapCe()
        {
            // Primary Key
            this.HasKey(t => t.InboxId);

            // Properties
            this.Property(t => t.InboxStreamType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.StreamType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.EventType)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(t => t.Payload)
                .IsRequired();

            this.Property(t => t.EventCollectionVersion)
                .IsRequired();

            this.Property(t => t.TransactionId)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Inbox");
            this.Property(t => t.InboxStreamType).HasColumnName("InboxStreamType");
            this.Property(t => t.InboxId).HasColumnName("InboxId");
            this.Property(t => t.EventId).HasColumnName("EventId");
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
