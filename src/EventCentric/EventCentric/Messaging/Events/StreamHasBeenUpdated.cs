using System;

namespace EventCentric.Messaging.Events
{
    public class StreamHasBeenUpdated : IMessage
    {
        public StreamHasBeenUpdated(Guid streamId, int updatedStreamVersion, int updatedStreamCollectionVersion)
        {
            this.StreamId = streamId;
            this.UpdatedStreamVersion = updatedStreamVersion;
            this.UpdatedStreamCollectionVersion = updatedStreamCollectionVersion;
        }

        public Guid StreamId { get; private set; }
        public int UpdatedStreamVersion { get; private set; }
        public int UpdatedStreamCollectionVersion { get; set; }
    }
}
