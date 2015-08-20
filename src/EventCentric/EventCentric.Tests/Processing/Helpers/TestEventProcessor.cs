using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace EventCentric.Tests.Processing.Helpers
{
    public class TestEventProcessor : EventProcessor<TestAggregate>,
        IEventHandler<TestQueuedEvent>
    {
        public TestEventProcessor(IBus bus, IEventStore<TestAggregate> store, ISubscriptionWriter subscriptionWriter)
            : base(bus, store, subscriptionWriter)
        { }

        public void Handle(TestQueuedEvent @event)
        {
            base.CreateNewStream(@event.StreamId, @event);
        }
    }
}
