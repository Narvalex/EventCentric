using System;
using System.Data.Entity;

namespace EventCentric.Persistence.SqlServer
{
    public interface IEventStoreDbContext : IDisposable
    {
        IDbSet<SnapshotEntity> Snapshots { get; }

        IDbSet<EventEntity> Events { get; }

        IDbSet<SubscriptionEntity> Subscriptions { get; }

        IDbSet<InboxEntity> Inbox { get; }

        int SaveChanges();
    }
}
