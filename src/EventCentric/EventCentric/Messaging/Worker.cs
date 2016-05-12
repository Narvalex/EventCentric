using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public abstract class Worker : ISystemHandler
    {
        protected readonly ISystemBus bus;

        public Worker(ISystemBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
            ((IBusRegistry)bus).Register(this);
        }
    }
}
