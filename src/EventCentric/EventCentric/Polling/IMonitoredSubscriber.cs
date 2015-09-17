namespace EventCentric.Polling
{
    public interface IMonitoredSubscriber
    {
        IMonitoredSubscription[] GetSubscriptionsMetrics();
    }
}
