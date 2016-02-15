using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Utils;

namespace EventCentric
{
    public abstract class MicroserviceWorker : Worker,
        IMessageHandler<FatalErrorOcurred>
    {
        protected volatile bool stopping;

        protected volatile bool systemHaltRequested = false;
        protected FatalErrorException fatalException = null;
        protected ILogger log;

        public MicroserviceWorker(IBus bus, ILogger log)
            : base(bus)
        {
            Ensure.NotNull(log, "log");

            this.log = log;
            Log = this.log;
        }

        public static ILogger Log { get; private set; }

        protected void Start()
        {
            this.stopping = false;
            this.OnStarting();
        }

        protected void Stop()
        {
            this.stopping = true;
            this.OnStopping();
        }

        protected virtual void OnStarting() { }

        protected virtual void OnStopping() { }

        public void Handle(FatalErrorOcurred message)
        {
            this.fatalException = message.Exception;
            this.systemHaltRequested = true;
            this.Stop();
        }
    }
}
