using EventCentric.Database;
using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class EventQueueDbContext : OptimizedDbContext, IEventQueueDbContext
    {
        public EventQueueDbContext(bool isReadOnly, string connectionString)
            : base(isReadOnly, connectionString)
        { }

        public EventQueueDbContext(string connectionString)
            : base(connectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
        }
    }
}
