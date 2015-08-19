using EventCentric.Repository;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Publishing
{
    public class StreamDao : IStreamDao
    {
        private readonly Func<ReadOnlyStreamDbContext> contextFactory;

        public StreamDao(Func<ReadOnlyStreamDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public string GetNextEventPayload(Guid streamId, int previousVersion)
        {
            using (var context = this.contextFactory())
            {
                return context
                        .Events
                        .Where(e => e.StreamId == streamId && e.Version > previousVersion)
                        .OrderBy(e => e.Version)
                        .First()
                        .Payload;
            }
        }

        public ConcurrentDictionary<Guid, int> GetStreamsVersionsById()
        {
            var streamsVersionById = new ConcurrentDictionary<Guid, int>();
            using (var context = this.contextFactory())
            {
                var streams = context
                                .Streams
                                .AsCachedAnyEnumerable();

                if (streams.Any())
                {
                    foreach (var stream in streams)
                        streamsVersionById.TryAdd(stream.StreamId, stream.Version);
                }

                return streamsVersionById;
            }
        }
    }
}

