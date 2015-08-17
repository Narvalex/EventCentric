using EventCentric.EventSourcing;

namespace EventCentric.Processing
{
    public interface ISubscriptionWriter
    {
        void LogIncomingEventAsReceivedAndIgnored(IEvent @event);
    }
}
