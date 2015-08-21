using EventCentric.Queueing;
using System;

namespace EventCentric.Tests.Queueing.Helpers
{
    public class TestEvent : QueuedEvent
    {
        public TestEvent()
            : base(Guid.Empty)
        { }

        public string Fact { get; set; }
    }
}
