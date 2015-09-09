using EventCentric.Queueing;
using EventCentric.Utils;

namespace EventCentric
{
    public abstract class ApplicationServiceBase
    {
        protected readonly IGuidProvider guid;
        protected readonly ITimeProvider time;

        public ApplicationServiceBase(IGuidProvider guid, ITimeProvider time)
        {
            Ensure.NotNull(guid, "guid");
            Ensure.NotNull(time, "time");

            this.guid = guid;
            this.time = time;
        }
    }

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
