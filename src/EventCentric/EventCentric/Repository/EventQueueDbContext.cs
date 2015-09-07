using EventCentric.Database;
using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventQueueDbContext : OptimizedDbContext<EventQueueDbContext>, IEventQueueDbContext
    {
        public EventQueueDbContext(bool isReadOnly, string nameOrconnectionString)
            : base(isReadOnly, nameOrconnectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
        }
    }
}
