using EventCentric.EventSourcing;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Messaging
{
    public class CrudEventQueue : EventQueue, ICrudEventQueue
    {
        public CrudEventQueue(string streamType, Func<bool, IEventQueueDbContext> contextFactory, ITextSerializer serializer, ITimeProvider time, IGuidProvider guid, IBus bus)
            : base(streamType, contextFactory, serializer, time, guid, bus)
        { }

        public void Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext
        {
            using (var context = base.contextFactory(false))
            {
                var versions = context.Events
                                      .Where(e => e.StreamId == @event.StreamId)
                                      .AsCachedAnyEnumerable();

                long currentVersion = 0;
                if (versions.Any())
                    currentVersion = versions.Max(e => e.Version);

                long updatedVersion = currentVersion + 1;

                var now = this.time.Now;

                @event.AsQueuedEvent(base.streamType, this.guid.NewGuid(), updatedVersion, now);

                context.Events.Add(
                    new EventEntity
                    {
                        StreamId = @event.StreamId,
                        Version = @event.Version,
                        EventId = @event.EventId,
                        TransactionId = @event.TransactionId,
                        EventType = @event.GetType().Name,
                        CreationDate = now,
                        Payload = this.serializer.Serialize(@event)
                    });

                performCrudOperation((T)context);

                context.SaveChanges();

                var version = context.Events.Max(e => e.EventCollectionVersion);

                this.bus.Publish(new EventStoreHasBeenUpdated(version));
            }
        }
    }
}
