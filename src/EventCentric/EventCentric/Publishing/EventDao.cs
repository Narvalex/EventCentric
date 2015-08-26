using EventCentric.Repository;
using EventCentric.Transport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Publishing
{
    public class EventDao : IEventDao
    {
        private readonly Func<EventStoreDbContext> contextFactory;

        public EventDao(Func<EventStoreDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public List<NewEvent> FindEvents(int previousEventCollectionVersion, int quantity)
        {
            using (var context = this.contextFactory())
            {
                var eventsQuery = context
                                    .Events
                                    .Where(e => e.EventCollectionVersion > previousEventCollectionVersion)
                                    .OrderBy(e => e.EventCollectionVersion);

                if (!eventsQuery.Any())
                    return null;

                var events = eventsQuery
                                .Select(e => new NewEvent(e.EventCollectionVersion, e.Payload))
                                .Take(quantity)
                                .ToList();
                return events;
            }
        }
    }

    public interface IEventDao
    {
        List<NewEvent> FindEvents(int previousEventCollectionVersion, int quantity);
    }
}
