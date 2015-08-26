using EventCentric.Polling;

namespace EventCentric.Transport
{
    public interface IHttpPoller
    {
        void PollSubscription(Subscription subscription);
    }
}
