using EventCentric.Messaging.Events;
using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public abstract class Worker : IWorker,
        IMessageHandler<FatalErrorOcurred>
    {
        protected volatile bool stopping;
        protected readonly IBus bus;

        protected volatile bool systemHaltRequested = false;
        protected FatalErrorException fatalException = null;

        protected Worker(IBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

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
