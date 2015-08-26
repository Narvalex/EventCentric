namespace EventCentric.Polling
{
    public class Subscription
    {
        public Subscription(string streamType, string url, int eventInProcessVersion, int eventCollectionVersion)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.EventInProcessVersion = eventInProcessVersion;
            this.EventCollectionVersion = eventCollectionVersion;
            this.IsPolling = false;
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }
        public volatile int EventInProcessVersion;
        public volatile int EventCollectionVersion;
        public volatile bool IsPolling;
    }
}
