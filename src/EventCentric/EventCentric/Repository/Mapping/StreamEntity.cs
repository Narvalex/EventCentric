using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public partial class SnapshotEntity
    {
        public string StreamType { get; set; }
        public Guid StreamId { get; set; }
        public long Version { get; set; }
        public string Payload { get; set; }
        public DateTime CreationLocalTime { get; set; }
        public DateTime UpdateLocalTime { get; set; }
    }

    public class StreamEntityMap : EntityTypeConfiguration<SnapshotEntity>
    {
        public StreamEntityMap()
        {
            // Primary Key
            this.HasKey(t => new { t.StreamType, t.StreamId });

            this.Property(t => t.StreamType).HasMaxLength(255);

            // Table & Column Mappings
            this.ToTable("Snapshots", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.Payload).HasColumnName("Payload");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
        }
    }

}
