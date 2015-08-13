using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.EntityFramework.Mapping
{
    public class StreamMap : EntityTypeConfiguration<Stream>
    {
        public StreamMap()
        {
            // Primary Key
            this.HasKey(t => t.StreamId);

            // Properties
            this.Property(t => t.Memento)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Streams", "EventStore");
            this.Property(t => t.StreamId).HasColumnName("StreamId");
            this.Property(t => t.Version).HasColumnName("Version");
            this.Property(t => t.Memento).HasColumnName("Memento");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
        }
    }
}
