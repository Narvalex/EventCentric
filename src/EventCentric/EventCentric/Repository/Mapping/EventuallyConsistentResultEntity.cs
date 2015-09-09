using System;
using System.Data.Entity.ModelConfiguration;

namespace EventCentric.Repository.Mapping
{
    public class EventuallyConsistentResult
    {
        /// <summary>
        /// The transaction identifier. The event of command that started the whole process will
        /// be the transaction identifier.
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// The result type is what the application service will parse in orther to understand the 
        /// outcoming of a transaction and will show to the user to provide a quick notification on 
        /// the status of the transaction outcome.
        /// </summary>
        public int ResultType { get; set; }

        /// <summary>
        /// Optionaly a message could be writed if an unexpected error ocurred;
        /// </summary>
        public string Message { get; set; }
    }

    public class EventuallyConsistentResultEntityMap : EntityTypeConfiguration<EventuallyConsistentResult>
    {
        public EventuallyConsistentResultEntityMap()
        {
            // Primary key
            this.HasKey(t => t.TransactionId);

            // Properties
            this.Property(t => t.Message)
                .IsOptional();

            // Table & Column Mappings
            this.ToTable("EventuallyConsistentResults", "EventStore");
            this.Property(t => t.TransactionId).HasColumnName("TransactionId");
            this.Property(t => t.ResultType).HasColumnName("ResultType");
            this.Property(t => t.Message).HasColumnName("Message");
        }
    }
}
