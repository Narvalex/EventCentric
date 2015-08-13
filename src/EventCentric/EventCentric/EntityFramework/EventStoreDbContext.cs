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

        public IDbSet<Event> Events { get; set; }
        public IDbSet<Inbox> Inbox { get; set; }
        public IDbSet<Stream> Streams { get; set; }
        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventMap());
            modelBuilder.Configurations.Add(new InboxMap());
            modelBuilder.Configurations.Add(new StreamMap());
            modelBuilder.Configurations.Add(new SubscriptionEntityMap());
        }
    }
}
