using EventCentric.EventSourcing;
using System.Linq;

namespace EventCentric.Utils.Testing
{
    public static class EventSourcedExtensions
    {
        public static TEvent SingleEvent<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return (TEvent)aggregate.PendingEvents.Single();
        }

        public static TAggregate Given<TAggregate>(this TAggregate aggregate, IEvent e)
            where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).On((dynamic)e);
            return aggregate;
        }

        public static TAggregate AndGiven<TAggregate>(this TAggregate aggregate, IEvent e)
            where TAggregate : IEventSourced
        {
            return aggregate.Given(e);
        }

        public static TAggregate When<TAggregate>(this TAggregate aggregate, IEvent e)
           where TAggregate : IEventSourced
        {
            ((dynamic)aggregate).Handle(e);
            return aggregate;
        }
    }
}
