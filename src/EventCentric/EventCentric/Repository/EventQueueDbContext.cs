using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    /// <summary>
    /// There could be an optimization just for reads..., maybe with an extension method.
    /// </summary>
    public class EventQueueDbContext : DbContext
    {
        static EventQueueDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);
        }

        public EventQueueDbContext(string nameOrconnectionString)
            : base(nameOrconnectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
        }
    }
}
