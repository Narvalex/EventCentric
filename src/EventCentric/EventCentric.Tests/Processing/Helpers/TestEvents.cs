using EventCentric.EventSourcing;
using EventCentric.Queueing;
using System;

namespace EventCentric.Tests.Processing.Helpers
{
    public class TestQueuedEvent : QueuedEvent
    {
        public TestQueuedEvent(string message, Guid streamId, string streamType)
            : base(streamId)
        {
            this.Message = message;
            base.StreamType = streamType;
        }

        public string Message { get; private set; }
    }

    public class TestEventHandled : Event
    {
        public TestEventHandled(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
