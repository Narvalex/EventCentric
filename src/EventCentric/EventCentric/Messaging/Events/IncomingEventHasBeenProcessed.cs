using System;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(Guid streamId, string streamType, int streamVersion)
        {
            this.StreamId = streamId;
            this.StreamType = StreamType;
            this.StreamVersion = streamVersion;
        }

        public Guid StreamId { get; private set; }
        public string StreamType { get; private set; }
        public int StreamVersion { get; private set; }
    }
}
