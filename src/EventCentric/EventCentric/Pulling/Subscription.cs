namespace EventCentric.Pulling
{
    public class Subscription
    {
        public string StreamType { get; set; }
        public string Url { get; set; }
        public int Version { get; set; }
        public bool IsPoisoned { get; set; }
        public bool IsBusy { get; set; }
    }
}
