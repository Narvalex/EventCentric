namespace EventCentric.Pulling
{
    public class SubscribedSource : Subscription
    {
        private volatile int streamCollectionVersion;

        public SubscribedSource(string streamType, string url, int streamCollectionVersion)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.streamCollectionVersion = streamCollectionVersion;
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }
        public int StreamCollectionVersion { get { return this.streamCollectionVersion; } }


        public void TryUpdateStreamCollectionVersion(int newVersion)
        {
            this.streamCollectionVersion = newVersion > this.streamCollectionVersion ? newVersion : this.streamCollectionVersion;
        }
    }
}
