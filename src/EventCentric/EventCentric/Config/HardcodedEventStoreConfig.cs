namespace EventCentric.Config
{
    public class HardcodedEventStoreConfig : IEventStoreConfig
    {
        public HardcodedEventStoreConfig(string connectionString, double longPollingTimeout, int pushMaxCount, string token)
        {
            this.ConnectionString = connectionString;
            this.LongPollingTimeout = longPollingTimeout;
            this.PushMaxCount = pushMaxCount;
            this.Token = token;
        }

        public HardcodedEventStoreConfig()
            : this("", 60000, 100, "")
        { }

        public string ConnectionString { get; }

        public double LongPollingTimeout { get; }

        public int PushMaxCount { get; }

        public string Token { get; }
    }
}
