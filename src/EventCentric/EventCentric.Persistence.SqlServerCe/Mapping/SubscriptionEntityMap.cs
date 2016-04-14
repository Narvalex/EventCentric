using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence
{
    public class SubscriptionEntityMapCe : EntityTypeConfiguration<SubscriptionEntity>
    {
        public SubscriptionEntityMapCe()
        {
            // Primary key
            this.HasKey(t => new { t.SubscriberStreamType, t.StreamType });

            // Properties
            this.Property(t => t.SubscriberStreamType).HasMaxLength(128);

            this.Property(t => t.Url)
                .IsRequired()
                .HasMaxLength(500);

            this.Property(t => t.ProcessorBufferVersion)
               .IsRequired();

            this.Property(t => t.IsPoisoned)
                .IsRequired();

            this.Property(t => t.WasCanceled)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Subscriptions");
            this.Property(t => t.SubscriberStreamType).HasColumnName("SubscriberStreamType");
            this.Property(t => t.StreamType).HasColumnName("StreamType");
            this.Property(t => t.Url).HasColumnName("Url");
            this.Property(t => t.Token).HasColumnName("Token");
            this.Property(t => t.ProcessorBufferVersion).HasColumnName("ProcessorBufferVersion");
            this.Property(t => t.IsPoisoned).HasColumnName("IsPoisoned");
            this.Property(t => t.WasCanceled).HasColumnName("WasCanceled");
            this.Property(t => t.PoisonEventCollectionVersion).HasColumnName("PoisonEventCollectionVersion");
            this.Property(t => t.DeadLetterPayload).HasColumnName("DeadLetterPayload");
            this.Property(t => t.ExceptionMessage).HasColumnName("ExceptionMessage");
            this.Property(t => t.CreationLocalTime).HasColumnName("CreationLocalTime");
            this.Property(t => t.UpdateLocalTime).HasColumnName("UpdateLocalTime");
        }
    }
}
