using EventCentric.Repository.Mapping;
using EventCentric.Utils;
using System;

namespace EventCentric.Repository
{
    public abstract class ViewDao<TDbContext> : IViewDao where TDbContext : EventuallyConsistentDbContext
    {
        protected readonly Func<TDbContext> contextFactory;

        public ViewDao(Func<TDbContext> contextFactory)
        {
            Ensure.NotNull(contextFactory, "View dao dbcontext factory");

            this.contextFactory = contextFactory;
        }

        public EventuallyConsistentResult AwaitTransactionResult(Guid transactionId)
            => this.Query(context => context.AwaitEventualConsistency(transactionId));

        protected T Query<T>(Func<TDbContext, T> queryPredicate)
        {
            using (var context = this.contextFactory.Invoke())
            {
                return queryPredicate(context);
            }
        }
    }
}
