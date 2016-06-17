namespace EventCentric.Config
{
    public class HardcodedEventStoreConfig : IEventStoreConfig
    {
        /// <summary>
        /// HardcodedEventStoreConfig constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="longPollingTimeout"></param>
        /// <param name="pushMaxCount"></param>
        /// <param name="token">We wait LESS than the publisher timeout, by default.</param>
        public HardcodedEventStoreConfig(string connectionString, double longPollingTimeout = 120000, int pushMaxCount = 100, string token = "")
        {
            this.ConnectionString = connectionString;
            this.LongPollingTimeout = longPollingTimeout;
            this.PushMaxCount = pushMaxCount;
            this.Token = token;
        }

        public HardcodedEventStoreConfig()
            : this("")
        { }

        public string ConnectionString { get; }

        public double LongPollingTimeout { get; }

        public int PushMaxCount { get; }

        public string Token { get; }
    }
}
