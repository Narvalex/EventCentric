using EventCentric.Persistence.SqlServer;
using EventCentric.Polling;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Publishing
{
    public class EventDao : IEventDao
    {
        private readonly Func<bool, IEventQueueDbContext> contextFactory;
        private readonly string streamType;

        public EventDao(Func<bool, IEventQueueDbContext> contextFactory, string streamType)
        {
            Ensure.NotNull(contextFactory, nameof(contextFactory));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));

            this.contextFactory = contextFactory;
            this.streamType = streamType;
        }

        /// <summary>
        /// FindEvents
        /// </summary>
        /// <returns>Events if found, otherwise return empty list.</returns>
        public List<NewRawEvent> FindEvents(long lastReceivedVersion, int quantity)
        {
            using (var context = this.contextFactory(true))
            {
                return context
                        .Events
                        .Where(e => e.StreamType == this.streamType && e.EventCollectionVersion > lastReceivedVersion)
                        .OrderBy(e => e.EventCollectionVersion)
                        .Take(quantity)
                        .ToList()
                        .Select(e => new NewRawEvent(e.EventCollectionVersion, e.Payload))
                        .ToList();

            }
        }

        public long GetEventCollectionVersion()
        {
            using (var context = this.contextFactory(true))
            {
                return !context.Events.Any(e => e.StreamType == this.streamType) ? 0 : context.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);
            }
        }
    }
}
