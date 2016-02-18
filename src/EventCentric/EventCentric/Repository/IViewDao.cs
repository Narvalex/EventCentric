using EventCentric.Repository.Mapping;
using System;

namespace EventCentric.Repository
{
    public interface IViewDao
    {
        EventuallyConsistentResult AwaitTransactionResult(Guid transactionId);
    }
}
