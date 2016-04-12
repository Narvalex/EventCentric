using System;
using System.Data.Entity;

namespace EventCentric.Persistence.SqlServer
{
    public interface IEventQueueDbContext : IDisposable
    {
        IDbSet<EventEntity> Events { get; }

        int SaveChanges();
    }
}
