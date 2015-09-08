using EventCentric.Queueing;
using EventCentric.Utils;

namespace EventCentric
{
    public abstract class ApplicationServiceBase
    {
        protected readonly IGuidProvider guid;

        public ApplicationServiceBase(IGuidProvider guid)
        {
            Ensure.NotNull(guid, "guid");

            this.guid = guid;
        }
    }

    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService : ApplicationServiceBase
    {
        protected readonly IEventBus bus;

        protected ApplicationService(IEventBus bus, IGuidProvider guid)
            : base(guid)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }
    }

    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class CrudApplicationService : ApplicationServiceBase
    {
        protected readonly ICrudEventBus bus;

        protected CrudApplicationService(ICrudEventBus bus, IGuidProvider guid)
            : base(guid)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }
    }
}
