using EventCentric.Publishing;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;

namespace EventCentric.Messaging
{
    public interface IInMemoryEventPublisherRegistry
    {
        void Register(IPollableEventSource publisher);
    }

    public interface IInMemoryEventPublisher : IInMemoryEventPublisherRegistry
    {
        PollResponse PollEvents(string streamType, long fromVersion, string consumerName);
        bool TryUpdateConsumer(string serverName, PollResponse response, out ServerStatus status);
    }
}
