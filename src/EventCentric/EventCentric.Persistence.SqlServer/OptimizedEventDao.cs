using EventCentric.Database;
using EventCentric.Polling;
using EventCentric.Publishing;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EventCentric.Persistence.SqlServer
{
    public class OptimizedEventDao : IEventDao
    {
        private readonly string streamType;
        private readonly SqlClientLite sql;
        private readonly Func<bool, IEventQueueDbContext> contextFactory;
        private readonly string findEventsQuery =
@"select top (@Quantity)
EventCollectionVersion,
Payload
from EventStore.Events
where StreamType = @StreamType
and EventCollectionVersion > @LastReceivedVersion
order by EventCollectionVersion";

        public OptimizedEventDao(Func<bool, IEventQueueDbContext> contextFactory, string connectionString, string streamType)
        {
            Ensure.NotNull(contextFactory, nameof(contextFactory));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(connectionString, nameof(connectionString));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));

            this.sql = new SqlClientLite(connectionString, timeoutInSeconds: 120);
            this.contextFactory = contextFactory;
            this.streamType = streamType;
        }

        /// <summary>
        /// FindEvents
        /// </summary>
        /// <returns>Events if found, otherwise return empty list.</returns>
        public List<NewRawEvent> FindEvents(long lastReceivedVersion, int quantity)
        {
            return this.sql.ExecuteReader(this.findEventsQuery,
                r => new NewRawEvent(r.GetInt64("EventCollectionVersion"), r.GetString("Payload")),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@StreamType", this.streamType),
                new SqlParameter("@LastReceivedVersion", lastReceivedVersion))
                .ToList();
        }

        // this is just on starting a publisher. No need to be optimized
        public long GetEventCollectionVersion()
        {
            using (var context = this.contextFactory(true))
            {
                return !context.Events.Any(e => e.StreamType == this.streamType) ? 0 : context.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);
            }
        }
    }
}
