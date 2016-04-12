using System;

namespace EventCentric.Persistence.SqlServer
{
    public interface IViewDao
    {
        EventuallyConsistentResult AwaitTransactionResult(Guid transactionId);
    }
}
