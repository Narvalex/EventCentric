using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventSource
    {
        PollResponse PollEvents(long eventBufferVersion, string consumerName);
    }
}
