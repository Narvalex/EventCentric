using EventCentric.Queueing;

namespace EventCentric.Tests.Queueing.Helpers
{
    public class TestEvent : QueuedEvent
    {
        public string Fact { get; set; }
    }
}
