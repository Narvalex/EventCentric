using EventCentric.Repository.Mapping;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventCentric.Repository
{
    public class EventuallyConsistentDbContext : EventStoreDbContext
    {
        private readonly TimeSpan timeout;

        public EventuallyConsistentDbContext(TimeSpan timeout, bool isReadOnly, string nameOrconnectionString)
            : base(isReadOnly, nameOrconnectionString)
        {
            this.timeout = timeout;
        }

        public IDbSet<EventuallyConsistentResult> EventuallyConsistentResults { get; set; }

        public EventuallyConsistentResult AwaitEventualConsistency(Guid transactionId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed < this.timeout)
            {
                if (this.EventuallyConsistentResults.Any(r => r.TransactionId == transactionId))
                    return this.EventuallyConsistentResults.Single(r => r.TransactionId == transactionId);

                Thread.Sleep(100);
            }

            throw new TimeoutException(string.Format("Timeout while awaiting eventual consistency for transaccion id {0}. Timeout: {1} seconds.", transactionId.ToString(), this.timeout.TotalSeconds));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new EventuallyConsistentResultEntityMap());
        }
    }
}
