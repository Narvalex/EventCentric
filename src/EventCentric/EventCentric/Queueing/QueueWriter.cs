using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Queueing
{
    public class QueueWriter<T> : IQueueWriter
    {
        private static readonly string _streamType = typeof(T).Name;
        private readonly Func<StreamDbContext> contextFactory;
        private readonly ITextSerializer serializer;
        private readonly ITimeProvider time;
        private readonly IGuidProvider guid;

        public QueueWriter(Func<StreamDbContext> contextFactory, ITextSerializer serializer, ITimeProvider time, IGuidProvider guid)
        {
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(time, "time");
            Ensure.NotNull(guid, "guid");

            this.contextFactory = contextFactory;
            this.serializer = serializer;
            this.time = time;
            this.guid = guid;
        }

        public Tuple<int, int> Enqueue(IEvent @event)
        {
            using (var context = this.contextFactory())
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

                context.Events.Add(
                    new EventEntity
                    {
                        StreamId = @event.StreamId,
                        Version = updatedVersion,
                        EventId = this.guid.NewGuid,
                        EventType = @event.GetType().Name,
                        CreationDate = now,
                        Payload = this.serializer.Serialize(@event)
                    });

                context.AddOrUpdate(
                    finder: () => context.Streams.Where(s => s.StreamId == @event.StreamId).SingleOrDefault(),
                    add: () => new StreamEntity
                    {
                        StreamId = @event.StreamId,
                        Version = @event.Version,
                        CreationDate = now
                    },
                    update: stream =>
                    {
                        stream.Version = @event.Version;
                        stream.CreationDate = now;
                    });

                context.SaveChanges();

                var updatedStreamCollectionVersion = context.Streams.Max(s => s.StreamCollectionVersion);

                return new Tuple<int, int>(updatedVersion, updatedStreamCollectionVersion);
            }
        }
    }
}
