using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventPublisher
    {
        string SourceName { get; }
        PollResponse PollEvents(long eventBufferVersion, string consumerName);
    }
}
