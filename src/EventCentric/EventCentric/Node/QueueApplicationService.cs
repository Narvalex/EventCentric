using EventCentric.Queueing;
using EventCentric.Utils;

namespace EventCentric
{
    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class QueueApplicationService : ApplicationServiceBase
    {
        protected readonly IEventQueue queue;

        protected QueueApplicationService(IEventQueue queue, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(queue, "queue");

            this.queue = queue;
        }
    }

    /// <summary>
    /// Domain driven design concept of application service.
    /// </summary>
    public abstract class CrudQueueApplicationService : ApplicationServiceBase
    {
        protected readonly ICrudEventQueue queue;

        protected CrudQueueApplicationService(ICrudEventQueue queue, IGuidProvider guid, ITimeProvider time)
            : base(guid, time)
        {
            Ensure.NotNull(queue, "queue");

            this.queue = queue;
        }
    }
}
