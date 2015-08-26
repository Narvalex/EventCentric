using System.Collections.Concurrent;

namespace EventCentric.Pulling
{
    public interface ISubscriptionRepository
    {
        ConcurrentBag<Subscription> GetSubscriptions();
    }
}
