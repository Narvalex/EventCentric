using EventCentric.Repository.Mapping;
using System;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventStoreDbContext : DbContext, IEventStoreDbContext
    {
        static EventStoreDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
        }

        public EventStoreDbContext(string nameOrconnectionString)
            : base(nameOrconnectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }
        public IDbSet<InboxEntity> Inbox { get; set; }
        public IDbSet<StreamEntity> Streams { get; set; }
        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }
        public IDbSet<SubscribedSourceEntity> SubscribedSources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
            modelBuilder.Configurations.Add(new InboxEntityMap());
            modelBuilder.Configurations.Add(new StreamEntityMap());
            modelBuilder.Configurations.Add(new SubscriptionEntityMap());
            modelBuilder.Configurations.Add(new SubscribedSourceEntityMap());
        }

        public void AddOrUpdate<T>(Func<T> finder, Func<T> add, Action<T> update) where T : class
        {
            var entity = finder.Invoke();

            if (entity == null)
            {
                entity = add.Invoke();
                this.Set<T>().Add(entity);
            }
            else
                update.Invoke(entity);
        }
    }
}
