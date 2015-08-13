using System;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(Guid streamId, int updatedVersion)
        {
            this.StreamId = streamId;
            this.UpdatedVersion = updatedVersion;
        }

        public Guid StreamId { get; private set; }
        public int UpdatedVersion { get; private set; }
    }
}
