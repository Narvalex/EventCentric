using System.Data.Entity;

namespace EventCentric.Persistence.SqlServerCe
{
    public class EventQueueCeDbContext : OptimizedDbContext, IEventQueueDbContext
    {
        public EventQueueCeDbContext(bool isReadOnly, string connectionString)
            : base(isReadOnly, connectionString)
        { }

        public EventQueueCeDbContext(string connectionString)
            : base(connectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMapCe());
        }
    }
}
