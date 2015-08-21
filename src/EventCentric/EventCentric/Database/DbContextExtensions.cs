using System;
using System.Data.Entity;

namespace EventCentric.Database
{
    public static class DbContextExtensions
    {
        public static void AddOrUpdate<T>(this DbContext context, Func<T> finder, Func<T> add, Action<T> update) where T : class
        {
            var entity = finder.Invoke();

            if (entity == null)
            {
                entity = add.Invoke();
                context.Set<T>().Add(entity);
            }
            else
                update.Invoke(entity);
        }

        public static void UnoptimizedAddOrUpdate<T>(this DbContext context, Func<T> finder, Func<T> add, Action<T> update) where T : class
        {
            var entity = finder.Invoke();

            if (entity == null) entity = add.Invoke();
            else update.Invoke(entity);

            context.TrackEntity(entity);
        }

        private static void TrackEntity<T>(this DbContext context, T entity) where T : class
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
                context.Set<T>().Add(entity);
        }
    }
}
