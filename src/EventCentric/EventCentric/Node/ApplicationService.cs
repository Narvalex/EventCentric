using EventCentric.Messaging;
using EventCentric.Utils;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService : ApplicationServiceBase
    {
        protected readonly IEventBus bus;

        protected ApplicationService(IEventBus bus, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }
    }
}
