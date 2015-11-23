using EventCentric.Database;
using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventStoreDbContext : OptimizedDbContext, IEventStoreDbContext
    {
        public EventStoreDbContext(bool isReadOnly, string connectionString)
            : base(isReadOnly, connectionString)
        { }

        public EventStoreDbContext(string connectionString)
            : base(connectionString)
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
