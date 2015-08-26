using System;

namespace EventCentric.Pulling
{
    public class SubscribedStream : OldSubscription
    {
        public string StreamType { get; private set; }
        public Guid StreamId { get; private set; }
        public int Version { get; private set; }
        public bool IsPoisoned { get; private set; }


        public SubscribedStream(string streamType, Guid streamId, int version, bool isPoisoned)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
            this.Version = version;
            this.IsPoisoned = isPoisoned;
        }

        public void TryUpdateVersion(int version)
        {
            this.Version = version > this.Version ? version : this.Version;
        }

        public void MarkAsPoisoned()
        {
            this.IsPoisoned = true;
        }
    }
}
