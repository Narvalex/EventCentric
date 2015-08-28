using EventCentric.EventSourcing;
using EventCentric.Messaging;
using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public interface ISubscriptionRepository
    {
        ConcurrentBag<BufferedSubscription> GetSubscriptions();

        void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception);
    }
}
