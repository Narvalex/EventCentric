using EventCentric.Repository.Mapping;
using System;

namespace EventCentric.Repository
{
    public interface IEventuallyConsistentDbContext : IEventStoreDbContext
    {
        EventuallyConsistentResultEntity AwaitEventualConsistency(Guid transactionId);
    }
}
