using EventCentric.Messaging;
using EventCentric.Pulling;
using EventCentric.Utils;
using System;

namespace EventCentric.Transport
{
    public class HttpPoller : IHttpPoller
    {
        private readonly IBus bus;

        public HttpPoller(IBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

        public void PollSubscription(Subscription subscription)
        {
            // when poll arives, publish in bus.

            throw new NotImplementedException();
        }
    }

    public interface IHttpPoller
    {
        void PollSubscription(Subscription subscription);
    }
}
