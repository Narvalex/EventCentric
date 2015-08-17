using EventCentric.EntityFramework.Mapping;
using System.Data.Entity;

namespace EventCentric.EntityFramework
{
    public class SubscriptionDbContext : DbContext
    {
        static SubscriptionDbContext()
        {
            System.Data.Entity.Database.SetInitializer<SubscriptionDbContext>(null);
        }

        public SubscriptionDbContext(string nameOrConnectionString = "Name=EventStoreDbContext")
            : base(nameOrConnectionString)
        { }

        public IDbSet<InboxEntity> Inbox { get; set; }
        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new InboxEntityMap());
            modelBuilder.Configurations.Add(new SubscriptionEntityMap());
        }
    }
}
