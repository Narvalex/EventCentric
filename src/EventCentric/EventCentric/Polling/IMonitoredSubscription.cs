namespace EventCentric.Polling
{
    public interface IMonitoredSubscription
    {
        string ProducerName { get; }
        int ConsumerVersion { get; }
        int ProducerVersion { get; }
        decimal UpToDatePercentage { get; }
    }
}
