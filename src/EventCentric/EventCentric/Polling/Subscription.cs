namespace EventCentric.Pulling
{
    public class Subscription
    {
        public Subscription(string streamType, string url, int eventInProcessVersion)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.EventInProcessVersion = eventInProcessVersion;
            this.IsPolling = false;
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }
        public volatile int EventInProcessVersion;
        public volatile bool IsPolling;
    }
}
