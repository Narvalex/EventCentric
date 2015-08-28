using EventCentric.EventSourcing;
using EventCentric.Transport;
using System.Collections.Concurrent;

namespace EventCentric.Polling
{
    public interface ISubscriptionRepository
    {
        ConcurrentBag<BufferedSubscription> GetSubscriptions();

        void FlagSubscriptionAsPoisoned(IncomingEvent<IEvent> poisonedEvent);
    }
}
