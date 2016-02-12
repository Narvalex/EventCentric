using EventCentric.Publishing;
using EventCentric.Transport;

namespace EventCentric.Messaging
{
    public interface IInMemoryEventPublisher
    {
        void Register(IEventPublisher publisher);
        PollResponse PollEvents(string streamType, long fromVersion, string consumerName);
    }
}
