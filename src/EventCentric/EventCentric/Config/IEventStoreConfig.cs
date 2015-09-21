namespace EventCentric.Config
{
    public interface IEventStoreConfig
    {
        string ConnectionString { get; }
        int PushMaxCount { get; }
        double LongPollingTimeout { get; }
        string Token { get; }
    }
}
