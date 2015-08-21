using EventCentric.EventSourcing;
using System;

namespace EventCentric.Queueing
{
    public class QueuedEvent : Event
    {
        public QueuedEvent(Guid streamId)
        {
            this.StreamId = streamId;
            this.StreamType = string.Empty;
            this.Version = default(int);
        }
    }
}
