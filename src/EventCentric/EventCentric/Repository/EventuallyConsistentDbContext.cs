using EventCentric.Repository.Mapping;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace EventCentric.Repository
{
    public class EventuallyConsistentDbContext : EventStoreDbContext
    {
        private readonly int maxAttemptsCount;

        public EventuallyConsistentDbContext(int maxAttemptsCount, bool isReadOnly, string nameOrconnectionString)
            : base(isReadOnly, nameOrconnectionString)
        {
            this.maxAttemptsCount = maxAttemptsCount;
        }

        public IDbSet<EventuallyConsistentResultEntity> EventuallyConsistentResults { get; set; }

        public EventuallyConsistentResultEntity AwaitEventualConsistency(Guid transactionId)
        {
            var attemptCounter = 1;
            while (attemptCounter <= this.maxAttemptsCount)
            {
                if (this.EventuallyConsistentResults.Any(r => r.TransactionId == transactionId))
                    return this.EventuallyConsistentResults.Single(r => r.TransactionId == transactionId);

                Thread.Sleep(100);
                attemptCounter += 1;
            }

            throw new TimeoutException(string.Format("Timeout while waiting eventual consistency for transaccion id {0} in attempt number {1}", transactionId.ToString(), attemptCounter));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new EventuallyConsistentResultEntityMap());
        }
    }
}
