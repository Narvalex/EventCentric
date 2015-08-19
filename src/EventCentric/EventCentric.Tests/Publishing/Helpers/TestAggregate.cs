using EventCentric.EventSourcing;
using System;

namespace EventCentric.Tests.Publishing.Helpers
{
    public class TestAggregate : EventSourced
    {
        protected TestAggregate(Guid id) : base(id)
        {
        }

        protected TestAggregate(Guid id, IMemento memento) : base(id, memento)
        {
        }
    }
}
