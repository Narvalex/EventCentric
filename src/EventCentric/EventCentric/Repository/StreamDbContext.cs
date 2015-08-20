using EventCentric.Repository.Mapping;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public class StreamDbContext : DbContext
    {
        static StreamDbContext()
        {
            System.Data.Entity.Database.SetInitializer<StreamDbContext>(null);
        }

        public StreamDbContext(string nameOrconnectionString)
            : base(nameOrconnectionString)
        { }

        public IDbSet<EventEntity> Events { get; set; }
        public IDbSet<StreamEntity> Streams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EventEntityMap());
            modelBuilder.Configurations.Add(new StreamEntityMap());
        }
    }
}
