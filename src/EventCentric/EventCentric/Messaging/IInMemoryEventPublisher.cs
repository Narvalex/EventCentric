using EventCentric.Publishing;
using EventCentric.Transport;

namespace EventCentric.Messaging
{
    public interface IInMemoryEventPublisher
    {
        void Register(IPollableEventSource publisher);
        PollResponse PollEvents(string streamType, long fromVersion, string consumerName);
    }
}
