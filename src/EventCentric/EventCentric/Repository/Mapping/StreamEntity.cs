using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public partial class StreamEntity
    {
        public Guid StreamId { get; set; }
        public int Version { get; set; }
        public string Memento { get; set; }
        public int StreamCollectionVersion { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateTime { get; set; }
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
            this.Property(t => t.Memento).HasColumnName("Memento");
            this.Property(t => t.StreamCollectionVersion).HasColumnName("StreamCollectionVersion");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.UpdateTime).HasColumnName("UpdateTime");
        }
    }

}
