using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public abstract class Worker
    {
        protected readonly IBus bus;

        public Worker(IBus bus)
        {
            Ensure.NotNull(bus, nameof(bus));

            this.bus = bus;
        }
    }
}
