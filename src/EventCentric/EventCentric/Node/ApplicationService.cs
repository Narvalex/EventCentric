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
        protected readonly IEventBus bus;

        protected ApplicationService(IEventBus bus, IGuidProvider guid)
        {
            Ensure.NotNull(guid, "guid");
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
            this.guid = guid;
        }
        protected Guid NewGuid()
        {
            return this.guid.NewGuid();
        }
    }
}
