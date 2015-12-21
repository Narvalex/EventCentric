using EventCentric.Messaging;
using EventCentric.Utils;
using System;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService
    {
        protected readonly IGuidProvider guid;
        protected readonly IUtcTimeProvider time;
        protected readonly IEventBus bus;

        protected ApplicationService(IEventBus bus, IGuidProvider guid, IUtcTimeProvider time)
        {
            Ensure.NotNull(guid, "guid");
            Ensure.NotNull(time, "time");
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
            this.guid = guid;
            this.time = time;
        }
        protected Guid NewGuid()
        {
            return this.guid.NewGuid();
        }

        protected DateTime Now { get { return this.time.Now; } }

    }
}
