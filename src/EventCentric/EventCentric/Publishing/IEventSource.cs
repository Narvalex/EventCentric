using EventCentric.Transport;

namespace EventCentric.Publishing
{
    public interface IEventSource
    {
        PollEventsResponse PollEvents(PollEventsRequest request);

        PollStreamsResponse PollStreams(PollStreamsRequest request);
    }
}
