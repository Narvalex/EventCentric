using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class ReadOnlyStreamDbContext : DbContext
    {
        static ReadOnlyStreamDbContext()
        {
            System.Data.Entity.Database.SetInitializer<ReadOnlyStreamDbContext>(null);
        }

        public ReadOnlyStreamDbContext(string nameOrconnectionString = "Name=ReadOnlyStreamDbContext")
            : base(nameOrconnectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        public IDbSet<EventEntity> Events { get; set; }
        public IDbSet<StreamEntity> Streams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
            modelBuilder.Configurations.Add(new StreamEntityMap());
        }
    }
}
