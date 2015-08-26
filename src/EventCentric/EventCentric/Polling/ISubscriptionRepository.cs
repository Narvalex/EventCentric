using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public interface ISubscriptionRepository
    {
        ConcurrentBag<Subscription> GetSubscriptions();
    }
}
