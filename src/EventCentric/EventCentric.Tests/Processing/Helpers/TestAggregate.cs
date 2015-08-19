using EventCentric.EventSourcing;
using System;

namespace EventCentric.Tests.Processing.Helpers
{
    public class TestAggregate : EventSourced
    {
        public TestAggregate(Guid id) : base(id)
        {
        }

        public TestAggregate(Guid id, IMemento memento) : base(id, memento)
        {
        }
    }
}
