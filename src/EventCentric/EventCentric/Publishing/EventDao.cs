using EventCentric.Repository;
using EventCentric.Transport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Publishing
{
    public class EventDao : IEventDao
    {
        private readonly Func<bool, IEventQueueDbContext> contextFactory;

        public EventDao(Func<bool, IEventQueueDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <summary>
        /// FindEvents
        /// </summary>
        /// <returns>Events if found, otherwise return empty list.</returns>
        public List<NewRawEvent> FindEvents(int lastReceivedVersion, int quantity)
        {
            using (var context = this.contextFactory(true))
            {
                var events = new List<NewRawEvent>();
                try
                {

                    var eventsQuery = context
                                .Events
                                .Where(e => e.EventCollectionVersion > lastReceivedVersion)
                                .OrderBy(e => e.EventCollectionVersion)
                                .Take(quantity);

                    foreach (var e in eventsQuery)
                        events.Add(new NewRawEvent(e.EventCollectionVersion, e.Payload));

                    return events;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return events;
                }
            }
        }

        public int GetEventCollectionVersion()
        {
            using (var context = this.contextFactory(true))
            {
                return !context.Events.Any() ? 0 : context.Events.Max(e => e.EventCollectionVersion);
            }
        }
    }
}
