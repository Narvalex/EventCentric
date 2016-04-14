using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence
{
    public class StreamEntityMapCe : EntityTypeConfiguration<SnapshotEntity>
    {
        public StreamEntityMapCe()
        {
            // Primary Key
            this.HasKey(t => new { t.StreamType, t.StreamId });

            this.Property(t => t.StreamType).HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Snapshots");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.Payload).HasColumnName("Payload");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
        }
    }

}
