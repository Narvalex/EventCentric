using System;
using System.Data.Entity;

namespace EventCentric.Persistence
{
    public interface IEventQueueDbContext : IDisposable
    {
        IDbSet<EventEntity> Events { get; }

        int SaveChanges();
    }
}
