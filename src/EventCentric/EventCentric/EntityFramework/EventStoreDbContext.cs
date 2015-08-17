using EventCentric.EntityFramework.Mapping;
using System.Data.Entity;

namespace EventCentric.EntityFramework
{
    public class EventStoreDbContext : DbContext
    {
        static EventStoreDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
        }

        public EventStoreDbContext(string nameOrconnectionString = "Name=EventStoreDbContext")
            : base(nameOrconnectionString)
        { }

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