using System;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(Guid streamId, int streamVersion)
        {
            this.StreamId = streamId;
            this.StreamVersion = streamVersion;
        }

        public Guid StreamId { get; private set; }
        public int StreamVersion { get; private set; }
    }
}
