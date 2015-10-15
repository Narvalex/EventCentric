using EventCentric.Messaging;
using EventCentric.Utils;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class ApplicationService : ApplicationServiceBase
    {
        protected readonly IEventBus queue;

        protected ApplicationService(IEventBus queue, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(queue, "queue");

            this.queue = queue;
        }
    }
}
