using EventCentric.Repository.Mapping;
using System;

namespace EventCentric.Repository
{
    public interface IEventuallyConsistentDbContext : IEventStoreDbContext
    {
        EventuallyConsistentResult AwaitEventualConsistency(Guid transactionId);
    }
}
