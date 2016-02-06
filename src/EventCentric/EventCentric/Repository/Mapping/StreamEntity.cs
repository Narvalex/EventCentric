using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public partial class StreamEntity
    {
        public Guid StreamId { get; set; }
        public long Version { get; set; }
        public string Snapshot { get; set; }
        public long StreamCollectionVersion { get; set; }
        public DateTime CreationLocalTime { get; set; }
        public DateTime UpdateLocalTime { get; set; }
    }

    public class StreamEntityMap : EntityTypeConfiguration<StreamEntity>
    {
        public StreamEntityMap()
        {
            // Primary Key
            this.HasKey(t => t.StreamId);

            this.Property(t => t.StreamCollectionVersion)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // Table & Column Mappings
            this.ToTable("Streams", "EventStore");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.Snapshot).HasColumnName("Snapshot");
            this.Property(t => t.StreamCollectionVersion).HasColumnName("StreamCollectionVersion");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
        }
    }

}
