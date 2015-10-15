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

                var currentVersion = 0;
                if (versions.Any())
                    currentVersion = versions.Max(e => e.Version);

                var updatedVersion = currentVersion + 1;

                var now = this.time.Now;

                ((Event)@event).StreamType = base.streamType;
                ((Event)@event).EventId = this.guid.NewGuid();
                ((Event)@event).Version = updatedVersion;

                context.Events.Add(
                    new EventEntity
                    {
                        StreamType = @event.StreamType,
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
