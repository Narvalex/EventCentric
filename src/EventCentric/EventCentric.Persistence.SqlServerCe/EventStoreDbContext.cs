using System.Data.Entity;

namespace EventCentric.Persistence.SqlServerCe
{
    public class EventStoreCeDbContext : OptimizedDbContext, IEventStoreDbContext
    {
        public EventStoreCeDbContext(bool isReadOnly, string connectionString)
            : base(isReadOnly, connectionString)
        { }

        public EventStoreCeDbContext(string connectionString)
            : base(connectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }
        public IDbSet<InboxEntity> Inbox { get; set; }
        public IDbSet<SnapshotEntity> Snapshots { get; set; }
        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMapCe());
            modelBuilder.Configurations.Add(new InboxEntityMapCe());
            modelBuilder.Configurations.Add(new StreamEntityMapCe());
            modelBuilder.Configurations.Add(new SubscriptionEntityMapCe());
        }
    }
}
