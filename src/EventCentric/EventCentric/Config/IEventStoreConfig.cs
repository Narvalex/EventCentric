namespace EventCentric.Config
{
    public interface IEventStoreConfig
    {
        string ConnectionString { get; }
        int PushMaxCount { get; }
        int PollAttemptsMaxCount { get; }
    }
}
