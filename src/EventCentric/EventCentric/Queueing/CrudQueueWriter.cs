using EventCentric.EventSourcing;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Queueing
{
    public class CrudQueueWriter<TAggregate> : QueueWriter<TAggregate>, ICrudQueueWriter
    {
        public CrudQueueWriter(Func<bool, IEventQueueDbContext> contextFactory, ITextSerializer serializer, ITimeProvider time, IGuidProvider guid)
            : base(contextFactory, serializer, time, guid)
        { }

        public int Enqueue<T>(IEvent @event, Action<T> performCrudOperation) where T : IEventQueueDbContext
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

                ((Event)@event).StreamType = _streamType;
                ((Event)@event).EventId = this.guid.NewGuid;
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

                return context.Events.Max(e => e.EventCollectionVersion);
            }
        }
    }
}
