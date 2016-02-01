namespace EventCentric.Querying
{
    public class EventSourceConnection
    {
        public EventSourceConnection(string streamType, string url, string token)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.Token = token;
        }

        public string StreamType { get; }
        public string Url { get; }
        public string Token { get; }
    }
}
