using EventCentric.Database;
using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class HeartbeatDbContext : OptimizedDbContext
    {
        public HeartbeatDbContext(bool isReadonly, string nameOrConnectionString)
            : base(isReadonly, nameOrConnectionString)
        { }

        //public HeartbeatDbContext()
        //    : base(false, "server=(local);Database=ClientesClient;User Id=sa;pwd =123456")
        //{ }

        public IDbSet<SubscriberHeartbeatEntity> SubscribersHeartbeats { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SubscriberHeartbeatEntityMap());
        }
    }
}
