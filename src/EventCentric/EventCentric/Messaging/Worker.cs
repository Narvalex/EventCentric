using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public abstract class Worker : IWorker
    {
        protected volatile bool stop;
        protected readonly IBus bus;

        protected Worker(IBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

        protected void Start()
        {
            this.stop = false;
            this.OnStarting();
        }

        protected void Stop()
        {
            this.stop = true;
            this.OnStopping();
        }

        protected virtual void OnStarting() { }

        protected virtual void OnStopping() { }
    }
}
