namespace EventCentric.Transport
{
    public interface IOldHttpPoller
    {
        OldPollEventsResponse PollEvents(string url);

        PollStreamsResponse PollStreams(string url);
    }
}
