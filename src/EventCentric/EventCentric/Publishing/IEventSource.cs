using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventSource
    {
        PollResponse PollEvents(PollRequest request);
    }
}
