using EventCentric.Database;
using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class HeartbeatDbContext : OptimizedDbContext
    {
        public HeartbeatDbContext(bool isReadonly, string connectionString)
            : base(isReadonly, connectionString)
        { }

        public HeartbeatDbContext(string connectionString)
            : base(connectionString)
        { }

        public IDbSet<SubscriberHeartbeatEntity> SubscribersHeartbeats { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SubscriberHeartbeatEntityMap());
        }
    }
}
