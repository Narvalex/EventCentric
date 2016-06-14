using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Persistence
{
    public class EventuallyConsistentResultEntityMap : EntityTypeConfiguration<EventuallyConsistentResult>
    {
        public EventuallyConsistentResultEntityMap()
        {
            // Primary key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Message)
                .IsOptional();

            // Table & Column Mappings
            this.ToTable("EventuallyConsistentResults", "EventStore");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.TransactionId).HasColumnName("TransactionId");
            this.Property(t => t.ResultType).HasColumnName("ResultType");
            this.Property(t => t.Message).HasColumnName("Message");
        }
    }
}
