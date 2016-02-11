using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Utils;

namespace EventCentric
{
    public abstract class MicroserviceBase : MicroserviceWorker
    {
        protected MicroserviceBase(string name, IBus bus, ILogger log)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(name, "name");

            this.Name = name;
        }

        public string Name { get; private set; }

        protected override void OnStarting()
        {
            var isRelease = true;
#if DEBUG
            isRelease = false;
#endif
            if (isRelease)
                this.log.Trace($"RELEASE build detected");
            else
                this.log.Trace($"DEBUG build detected");
        }
    }
}
