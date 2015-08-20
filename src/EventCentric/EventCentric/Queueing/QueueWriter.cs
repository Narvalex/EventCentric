using EventCentric.EventSourcing;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Queueing
{
    public class QueueWriter : IQueueWriter
    {
        private readonly Func<StreamDbContext> contextFactory;
        private readonly ITextSerializer serializer;
        private readonly ITimeProvider time;

        public QueueWriter(Func<StreamDbContext> contextFactory, ITextSerializer serializer, ITimeProvider time)
        {
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(time, "time");

            this.contextFactory = contextFactory;
            this.serializer = serializer;
        }

        public int EnqueueMessage(IEvent message)
        {
            using (var context = this.contextFactory())
            {
                var versions = context.Events
                                      .Where(e => e.StreamId == message.StreamId)
                                      .AsCachedAnyEnumerable();

                var currentVersion = 0;
                if (versions.Any())
                    currentVersion = versions.Max(e => e.Version);

                var updatedVersion = currentVersion + 1;

                var now = this.time.Now;
                context.Events.Add(
                    new EventEntity
                    {
                        StreamId = message.EventId,
                        Version = updatedVersion,
                        EventId = message.EventId,
                        EventType = message.GetType().Name,
                        CreationDate = now,
                        Payload = this.serializer.Serialize(message)
                    });

                context.AddOrUpdate(
                    entityFinder: () => context.Streams.Where(s => s.StreamId == message.StreamId).SingleOrDefault(),
                    newEntityToAdd: new StreamEntity
                    {
                        StreamId = message.StreamId,
                        Version = message.Version,
                        CreationDate = now
                    },
                    updateEntity: stream =>
                    {
                        stream.Version = message.Version;
                        stream.CreationDate = now;
                    });

                context.SaveChanges();

                return updatedVersion;
            }
        }
    }
}
