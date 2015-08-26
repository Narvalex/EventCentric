using EventCentric.Repository;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Publishing
{
    public class OldStreamDao : IOldStreamDao
    {
        private readonly Func<EventQueueDbContext> contextFactory;

        public OldStreamDao(Func<EventQueueDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public string GetNextEventPayload(Guid streamId, int previousVersion)
        {
            using (var context = this.contextFactory())
            {
                try
                {
                    return context
                            .Events
                            .Where(e => e.StreamId == streamId && e.Version > previousVersion)
                            .OrderBy(e => e.Version)
                            .First()
                            .Payload;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        public Tuple<Guid, int> GetNextStreamIdAndStreamCollectionVersion(int previousStreamCollectionVersion)
        {
            using (var context = this.contextFactory())
            {
                var nextStream =
                    context
                    .Streams
                    .Where(s => s.StreamCollectionVersion > previousStreamCollectionVersion)
                    .OrderBy(s => s.StreamCollectionVersion)
                    .First();

                return new Tuple<Guid, int>(nextStream.StreamId, nextStream.StreamCollectionVersion);
            }
        }

        public int GetStreamCollectionVersion()
        {
            using (var context = this.contextFactory())
            {
                if (context.Streams.Any())
                    return context.Streams.Max(s => s.StreamCollectionVersion);

                return 0;
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

