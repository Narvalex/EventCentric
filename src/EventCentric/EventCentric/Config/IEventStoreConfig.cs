namespace EventCentric.Config
{
    public interface IEventStoreConfig
    {
        string ConnectionString { get; }
        int PushMaxCount { get; }

        /// <summary>
        /// The timeout of the long polling mechanism that the publisher implements, in milliseconds.
        /// </summary>
        double LongPollingTimeout { get; }
        string Token { get; }
    }
}
