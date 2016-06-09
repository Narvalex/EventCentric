using EventCentric.EventSourcing;
using EventCentric.Messaging;

namespace EventCentric.Polling
{
    public interface ISubscriptionRepository
    {
        SubscriptionBuffer[] GetSubscriptions();

        void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception);

        bool TryAddNewSubscriptionOnTheFly(string streamType, string url, string token);

        void PersistSubscriptionVersion(string subscription, long version);
    }
}
