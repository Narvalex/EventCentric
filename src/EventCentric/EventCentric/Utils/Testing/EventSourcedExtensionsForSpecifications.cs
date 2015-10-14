using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Utils.Testing
{
    public static class EventSourcedExtensionsForSpecifications
    {
        public static TAggregate On<TAggregate>(this TAggregate aggregate, IEvent e)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).On((dynamic)e);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e)
           where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle(e);
            return aggregate;
        }

        public static TEvent ExpectSingleEventOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return (TEvent)aggregate.PendingEvents.Single();
        }

        public static TEvent ExpectOneEventOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return aggregate.PendingEvents.OfType<TEvent>().Single();
        }

        public static IEnumerable<TEvent> ExpectEventsOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return aggregate.PendingEvents.OfType<TEvent>().AsEnumerable();
        }

        public static TAggregate ThenExpectOneEventOfType<TAggregate, TEvent>(this TAggregate aggregate, TEvent e)
           where TAggregate : IEventSourced
           where TEvent : IEvent
        {
            var singleEvent = aggregate.PendingEvents.OfType<TEvent>().Single();
            return aggregate;
        }

        public static TAggregate ThenExpectMultipleEventsOfType<TAggregate, TEvent>(this TAggregate aggregate, TEvent e)
           where TAggregate : IEventSourced
           where TEvent : IEvent
        {
            var hasEvents = aggregate.PendingEvents.OfType<TEvent>().Any();
            if (hasEvents)
                return aggregate;
            else
                throw new InvalidOperationException($"Aggregate of type {aggregate.GetType().Name} does not generated any event of type {e.GetType().Name} so far.");
        }
    }
}
