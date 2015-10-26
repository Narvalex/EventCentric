using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventSource
    {
        PollResponse PollEvents(int eventBufferVersion, string consumerName);
    }
}
