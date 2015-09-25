using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Node
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ProcessorApplicationService : ApplicationServiceBase
    {
        protected readonly IBus bus;

        protected ProcessorApplicationService(IBus bus, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

        protected void Process(IEvent @event)
        {
            this.bus.Send(new NewIncomingEvents(new IEvent[] { @event }));
        }

        protected void Process(IEnumerable<IEvent> events)
        {
            this.bus.Send(new NewIncomingEvents(events.ToArray()));
        }
    }
}
