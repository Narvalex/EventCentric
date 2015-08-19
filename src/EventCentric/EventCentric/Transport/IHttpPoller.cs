namespace EventCentric.Transport
{
    public interface IHttpPoller
    {
        PollResponse Poll(string url);
    }
}
