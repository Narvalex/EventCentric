using System;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public static class DbContextExtensions
    {
        public static void AddOrUpdate<T>(this DbContext context, Func<T> entityFinder, T newEntityToAdd, Action<T> updateEntity) where T : class
        {
            var entity = entityFinder.Invoke();

            if (entity == null)
                context.AddOrUpdate(newEntityToAdd);
            else
            {
                updateEntity.Invoke(entity);
                context.AddOrUpdate(entity);
            }
        }

        public static void AddOrUpdate<T>(this DbContext context, T entity) where T : class
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
                context.Set<T>().Add(entity);
        }
    }
}
