using EventCentric.Repository;
using EventCentric.Transport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Publishing
{
    public class EventDao : IEventDao
    {
        private readonly Func<EventQueueDbContext> contextFactory;

        public EventDao(Func<EventQueueDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public List<NewEvent> GetEvents(int fromEventCollectionVersion, int quantity)
        {
            using (var context = this.contextFactory())
            {
                return context
                        .Events
                        .Where(e => e.EventCollectionVersion >= fromEventCollectionVersion)
                        .OrderBy(e => e.EventCollectionVersion)
                        .Select(e => new NewEvent(e.EventCollectionVersion, e.Payload))
                        .Take(quantity)
                        .ToList();
            }
        }

        public int GetEventCollectionVersion()
        {
            using (var context = this.contextFactory())
            {
                return !context.Events.Any() ? 0 : context.Events.Max(e => e.EventCollectionVersion);
            }
        }
    }

    public interface IEventDao
    {
        List<NewEvent> GetEvents(int fromEventCollectionVersion, int quantity);

        int GetEventCollectionVersion();
    }
}
