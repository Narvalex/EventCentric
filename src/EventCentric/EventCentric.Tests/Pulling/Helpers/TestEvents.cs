using EventCentric.EventSourcing;
using System;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestEvent1 : IEvent
    {
        public Guid StreamId { get; set; }

        public string StreamType
        {
            get { return "Clients"; }
        }
    }
}
