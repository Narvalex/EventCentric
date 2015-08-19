using EventCentric.Messaging;
using EventCentric.Repository;
using System;

namespace EventCentric.EventSourcing
{
    public interface ISubscriptionWriter
    {
        void LogIncomingEventAsIgnored(IEvent @event);

        void LogIncomingEvent(IEvent @event, EventStoreDbContext context, bool ignored = false);

        void LogPosisonedMessage(string streamType, Guid streamId, PoisonMessageException exception);
    }
}
