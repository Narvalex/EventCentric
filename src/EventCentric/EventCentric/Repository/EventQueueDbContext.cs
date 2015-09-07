using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventQueueDbContext : DbContext, IEventQueueDbContext
    {
        static EventQueueDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);
        }

        public EventQueueDbContext(bool isReadOnly, string nameOrconnectionString)
            : base(nameOrconnectionString)
        {
            if (isReadOnly)
                this.Configuration.AutoDetectChangesEnabled = false;
        }

        public IDbSet<EventEntity> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
        }
    }
}
