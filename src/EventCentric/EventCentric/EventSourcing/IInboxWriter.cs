namespace EventCentric.EventSourcing
{
    public interface IInboxWriter
    {
        void LogIncomingEventAsReceivedAndIgnored(IEvent @event);
    }
}
