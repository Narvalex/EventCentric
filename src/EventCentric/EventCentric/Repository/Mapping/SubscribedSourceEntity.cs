using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class SubscribedSourceEntity
    {
        public string StreamType { get; set; }
        public string Url { get; set; }
        public int StreamCollectionVersion { get; set; }
    }

    public class SubscribedSourceEntityMap : EntityTypeConfiguration<SubscribedSourceEntity>
    {
        public SubscribedSourceEntityMap()
        {
            // Primary key
            this.HasKey(t => t.StreamType);

            // Properties
            this.Property(t => t.Url)
                .IsRequired()
                .HasMaxLength(500);

            // Table & Column Mappings
            this.ToTable("SubscribedSources", "EventStore");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.StreamCollectionVersion).HasColumnName("StreamCollectionVersion");
        }
    }
}
