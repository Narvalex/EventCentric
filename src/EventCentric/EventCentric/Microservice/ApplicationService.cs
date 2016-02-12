using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Microservice;
using EventCentric.Utils;
using System;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService : IEventSource
    {
        protected readonly IGuidProvider guid;
        protected readonly IServiceBus bus;
        protected readonly ILogger log;

        public ApplicationService(IServiceBus bus, IGuidProvider guid, ILogger log)
        {
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(bus, nameof(bus));
            Ensure.NotNull(log, nameof(log));

            this.bus = bus;
            this.guid = guid;
            this.log = log;
        }
        protected Guid NewGuid()
        {
            return this.guid.NewGuid();
        }
    }
}
