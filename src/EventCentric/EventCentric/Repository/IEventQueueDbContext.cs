using EventCentric.Repository.Mapping;
using System;
using System.Data.Entity;

namespace EventCentric.Repository
{
    public interface IEventQueueDbContext : IDisposable
    {
        IDbSet<EventEntity> Events { get; }

        int SaveChanges();
    }
}
