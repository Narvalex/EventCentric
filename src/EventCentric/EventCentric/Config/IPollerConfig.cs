namespace EventCentric.Config
{
    public interface IPollerConfig
    {
        int BufferQueueMaxCount { get; }
        int EventsToFlushMaxCount { get; }
        double Timeout { get; }
    }
}
