using System;

namespace EventCentric.Messaging.Events
{
    public class StreamHasBeenUpdated : IMessage
    {
        public StreamHasBeenUpdated(Guid streamId, int updatedStreamVersion)
        {
            this.StreamId = streamId;
            this.UpdatedStreamVersion = updatedStreamVersion;
        }

        public Guid StreamId { get; private set; }
        public int UpdatedStreamVersion { get; private set; }
    }
}
