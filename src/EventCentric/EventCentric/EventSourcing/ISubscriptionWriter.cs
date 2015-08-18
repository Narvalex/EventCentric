using EventCentric.Repository;

namespace EventCentric.EventSourcing
{
    public interface ISubscriptionWriter
    {
        void LogIncomingEventAsIgnored(IEvent @event);

        void LogIncomingEvent(IEvent @event, EventStoreDbContext context, bool ignored = false);
    }
}
