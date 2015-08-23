using EventCentric.EventSourcing;
using System;

namespace EventCentric.Tests.Processing.Helpers
{
    public class TestAggregate : EventSourced,
        IHandles<TestQueuedEvent>,
        IUpdatesOn<TestEventHandled>
    {
        public TestAggregate(Guid id) : base(id)
        { }

        public TestAggregate(Guid id, IMemento memento) : base(id, memento)
        { }

        public void Handle(TestQueuedEvent e)
        {
            base.Publish(new TestEventHandled(e.Message));
        }

        public void On(TestEventHandled e)
        { }
    }
}
