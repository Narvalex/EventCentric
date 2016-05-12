using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public abstract class Worker : ISystemHandler
    {
        protected readonly IBus bus;

        public Worker(IBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
            ((IBusRegistry)bus).Register(this);
        }
    }
}
