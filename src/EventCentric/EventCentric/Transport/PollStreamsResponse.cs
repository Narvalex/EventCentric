using System;

namespace EventCentric.Transport
{
    public class PollStreamsResponse
    {
        public PollStreamsResponse(bool newStreamWasFound, Guid? newStreamId, int? updatedStreamCollectionVersion)
        {
            this.NewStreamWasFound = newStreamWasFound;
            this.NewStreamId = newStreamId;
            this.UpdatedStreamCollectionVersion = updatedStreamCollectionVersion;
        }

        public bool NewStreamWasFound { get; private set; }
        public Guid? NewStreamId { get; private set; }
        public int? UpdatedStreamCollectionVersion { get; private set; }
    }
}
