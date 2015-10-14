using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Utils.Testing
{
    public static class EventSourcedExtensionsForSpecifications
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

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4)
           where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14, (dynamic)s15);
            return aggregate;
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15, IDomainService s16)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle((dynamic)e, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14, (dynamic)s15, (dynamic)s16);
            return aggregate;
        }

        #endregion
    }
}
