using EventCentric.EventSourcing;
using EventCentric.Messaging;

namespace EventCentric.Polling
{
    public interface ISubscriptionRepository
    {
        SubscriptionBuffer[] GetSubscriptions();

        void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception);
    }
}
