namespace EventCentric.Transport
{
    public interface IHttpPollster
    {
        void PollSubscription(string streamType, string url, int lastReceivedVersion);
    }
}
