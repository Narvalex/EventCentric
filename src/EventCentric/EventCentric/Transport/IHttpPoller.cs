namespace EventCentric.Transport
{
    public interface IHttpPoller
    {
        PollEventsResponse PollEvents(string url);

        PollStreamsResponse PollStreams(string url);
    }
}
