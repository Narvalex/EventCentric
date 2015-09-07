using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventStoreDbContext : DbContext, IEventStoreDbContext
    {
        static EventStoreDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
        }

        public EventStoreDbContext(bool isReadOnly, string nameOrconnectionString)
            : base(nameOrconnectionString)
        {
            if (isReadOnly)
                this.Configuration.AutoDetectChangesEnabled = false;
        }

        public IDbSet<EventEntity> Events { get; set; }
        public IDbSet<InboxEntity> Inbox { get; set; }
        public IDbSet<StreamEntity> Streams { get; set; }
        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
            modelBuilder.Configurations.Add(new InboxEntityMap());
            modelBuilder.Configurations.Add(new StreamEntityMap());
            modelBuilder.Configurations.Add(new SubscriptionEntityMap());
        }
    }
}
