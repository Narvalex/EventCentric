using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class ReadOnlySubscriptionDbContext : DbContext
    {
        static ReadOnlySubscriptionDbContext()
        {
            System.Data.Entity.Database.SetInitializer<ReadOnlySubscriptionDbContext>(null);
        }

        public ReadOnlySubscriptionDbContext(string nameOrconnectionString = "Name=EventStoreDbContext")
            : base(nameOrconnectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        public IDbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SubscriptionEntityMap());
        }
    }
}
