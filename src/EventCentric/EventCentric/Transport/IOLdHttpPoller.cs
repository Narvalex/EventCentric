namespace EventCentric.Transport
{
    public interface IOldHttpPoller
    {
        PollEventsResponse PollEvents(string url);

        PollStreamsResponse PollStreams(string url);
    }
}
