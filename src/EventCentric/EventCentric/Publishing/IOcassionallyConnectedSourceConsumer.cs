using EventCentric.Publishing.Dto;
using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IOcassionallyConnectedSourceConsumer
    {
        string SourceName { get; }

        ServerStatus UpdateServer(PollResponse response);
    }
}
