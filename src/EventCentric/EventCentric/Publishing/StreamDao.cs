using EventCentric.EntityFramework;
using System;
using System.Collections.Generic;
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

        public IEnumerable<KeyValuePair<Guid, int>> GetStreamsVersionsById()
        {
            using (var context = this.contextFactory())
            {
                return context
                        .Streams
                        .Select(s => new KeyValuePair<Guid, int>(s.StreamId, s.Version))
                        .AsEnumerable();
            }
        }
    }
}
