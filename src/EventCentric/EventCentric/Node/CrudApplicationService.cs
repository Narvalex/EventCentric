using EventCentric.Messaging;
using EventCentric.Utils;

namespace EventCentric.Node
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class CrudApplicationService : ApplicationServiceBase
    {
        protected readonly ICrudEventBus bus;

        protected CrudApplicationService(ICrudEventBus bus, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }
    }
}
