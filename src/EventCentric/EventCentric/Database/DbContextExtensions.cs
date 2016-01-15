using System;
using System.Data.Entity;

namespace EventCentric.Database
{
    public static class DbContextExtensions
    {
        public static TEntity AddOrUpdate<TEntity, TDbContext>(this TDbContext context, Func<TEntity> find, Func<TEntity> add, Action<TEntity> update)
            where TEntity : class
            where TDbContext : DbContext
        {
            var entity = find.Invoke();

            if (entity == null)
            {
                entity = add.Invoke();
                context.Set<TEntity>().Add(entity);
            }
            else
                update.Invoke(entity);

            return entity;
        }

        public static void UnoptimizedAddOrUpdate<TEntity, TDbContext>(this DbContext context, Func<TEntity> find, Func<TEntity> add, Action<TEntity> update)
            where TEntity : class
            where TDbContext : DbContext
        {
            var entity = find.Invoke();

            if (entity == null) entity = add.Invoke();
            else update.Invoke(entity);

            context.TrackEntity(entity);
        }

        private static void TrackEntity<TEntity, TDbContext>(this TDbContext context, TEntity entity)
            where TEntity : class
            where TDbContext : DbContext
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
                context.Set<TEntity>().Add(entity);
        }
    }
}
