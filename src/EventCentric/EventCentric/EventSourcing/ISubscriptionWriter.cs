using EventCentric.EntityFramework;

namespace EventCentric.EventSourcing
{
    public interface ISubscriptionWriter
    {
        void LogIncomingEventAsIgnored(IEvent @event);

        void LogIncomingEvent(IEvent @event, EventStoreDbContext context, bool ignored = false);
    }
}
