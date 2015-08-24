using EventCentric.Messaging;
using System;

namespace EventCentric.EventSourcing
{
    public interface ISubscriptionInboxWriter
    {
        void CreateNewSubscription(string streamType, Guid streamId, int updatedStreamCollectionVersion);

        void LogIncomingEventAsIgnored(IEvent @event);

        void LogPosisonedMessage(string streamType, Guid streamId, PoisonMessageException exception);
    }
}
