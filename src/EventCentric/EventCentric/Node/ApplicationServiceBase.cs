using EventCentric.Utils;
using System;

namespace EventCentric
{
    public abstract class ApplicationServiceBase
    {
        protected readonly IGuidProvider guid;
        protected readonly ITimeProvider time;

        protected ApplicationServiceBase(IGuidProvider guid, ITimeProvider time)
        {
            Ensure.NotNull(guid, "guid");
            Ensure.NotNull(time, "time");

            this.guid = guid;
            this.time = time;
        }

        protected Guid NewGuid { get { return this.guid.NewGuid(); } }

        protected DateTime Now { get { return this.time.Now; } }
    }
}
