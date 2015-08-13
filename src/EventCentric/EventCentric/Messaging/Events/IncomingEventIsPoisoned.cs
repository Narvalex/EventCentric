using System;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventIsPoisoned : IMessage
    {
        public IncomingEventIsPoisoned(string streamType, Guid streamId)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
        }

        public string StreamType { get; private set; }
        public Guid StreamId { get; set; }
    }
}
