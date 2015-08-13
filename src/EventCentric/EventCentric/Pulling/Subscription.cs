using System;

namespace EventCentric.Pulling
{
    public class Subscription
    {
        public string StreamType { get; private set; }
        public Guid StreamId { get; private set; }
        public string Url { get; private set; }
        public int Version { get; private set; }
        public bool IsPoisoned { get; private set; }
        public bool IsBusy { get; private set; }

        public Subscription(string streamType, Guid streamId, string url, int version, bool isPoisoned)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
            this.Url = url;
            this.Version = version;
            this.IsPoisoned = IsPoisoned;
            this.IsBusy = false;
        }

        public void EnterBusy()
        {
            this.IsBusy = true;
        }

        public void ExitBusy()
        {
            this.IsBusy = false;
        }

        public void UpdateVersion(int version)
        {
            this.Version = version;
        }

        public void MarkAsPoisoned()
        {
            this.IsPoisoned = true;
        }
    }
}
