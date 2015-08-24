using EventCentric.Pulling;
using System;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventIsPoisoned : IMessage
    {
        public IncomingEventIsPoisoned(string streamType, Guid streamId, PoisonMessageException exception)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
            this.Exception = exception;
        }

        public string StreamType { get; private set; }
        public Guid StreamId { get; private set; }
        public PoisonMessageException Exception { get; private set; }
    }
}
