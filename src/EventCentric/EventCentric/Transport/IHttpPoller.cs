namespace EventCentric.Transport
{
    public interface IHttpPoller
    {
        void PollSubscription(string streamType, string url, int lastReceivedVersion);
    }
}
