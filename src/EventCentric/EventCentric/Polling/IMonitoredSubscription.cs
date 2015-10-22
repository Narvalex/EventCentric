namespace EventCentric.Polling
{
    public interface IMonitoredSubscription
    {
        string ProducerName { get; }
        long ConsumerVersion { get; }
        long ProducerVersion { get; }
        decimal UpToDatePercentage { get; }
    }
}
