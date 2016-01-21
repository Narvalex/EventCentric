using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Utils.Testing
{
    public static class EventSourcedAggregateExtensions
    {
        public static TAggregate GivenOn<TAggregate>(this TAggregate aggregate, IEvent e)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).On((dynamic)e);
            return aggregate;
        }

        public static TEvent SingleEventOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return (TEvent)aggregate.PendingEvents.Single();
        }

        public static TEvent OneEventOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return aggregate.PendingEvents.OfType<TEvent>().Single();
        }

        public static IEnumerable<TEvent> EventsOfType<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return aggregate.PendingEvents.OfType<TEvent>().AsEnumerable();
        }

        public static IEventSourced ThenExpectSingle<TEvent>(this IEventSourced aggregate)
        {
            var count = aggregate.PendingEvents.OfType<TEvent>().Count();

            if (count <= 0)
                throw new InvalidOperationException($"There was expected to find a single event of type '{typeof(TEvent).Name}' but none were found.");

            else if (count == 1)
                return aggregate;

            throw new InvalidOperationException($"There was expected to find a single event of type '{typeof(TEvent).Name}' but there where found {count} events of that type.");
        }

        public static IEventSourced ThenExpectOne<TEvent>(this IEventSourced aggregate)
           where TEvent : IEvent
        {
            var singleEvent = aggregate.PendingEvents.OfType<TEvent>().Single();
            return aggregate;
        }

        public static IEventSourced ThenExpectAtLeastOne<TEvent>(this IEventSourced aggregate)
           where TEvent : IEvent
        {
            var hasEvents = aggregate.PendingEvents.OfType<TEvent>().Any();
            if (hasEvents)
                return aggregate;
            else
                throw new InvalidOperationException($"Aggregate of type {aggregate.GetType().Name} does not generated any event of type {typeof(TEvent).Name} so far.");
        }

        public static IEventSourced AndOne<TEvent>(this IEventSourced aggregate)
           where TEvent : IEvent
        {
            var singleEvent = aggregate.PendingEvents.OfType<TEvent>().Single();
            return aggregate;
        }

        public static IEventSourced AndAtLeastOne<TEvent>(this IEventSourced aggregate)
           where TEvent : IEvent
        {
            var hasEvents = aggregate.PendingEvents.OfType<TEvent>().Any();
            if (hasEvents)
                return aggregate;
            else
                throw new InvalidOperationException($"Aggregate of type {aggregate.GetType().Name} does not generated any event of type {typeof(TEvent).Name} so far.");
        }

        public static IEventSourced AndNotAny<TEvent>(this IEventSourced aggregate)
           where TEvent : IEvent
        {
            if (aggregate.PendingEvents.OfType<TEvent>().Any())
                throw new InvalidOperationException($"Not expected any '{typeof(TEvent).Name}' event type but there is/are {aggregate.PendingEvents.OfType<TEvent>().Count()} event/s of that type.");
            return aggregate;
        }

        #region When

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e);
            return aggregate;
        }

        #endregion
    }
}
