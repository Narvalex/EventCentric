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
}
