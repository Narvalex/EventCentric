using EventCentric.Publishing.Dto;
using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IOcassionallyConnectedSourceConsumer
    {
        string SourceName { get; }

        string ConsumerName { get; }

        ServerStatus UpdateConsumer(PollResponse response);
    }
}
