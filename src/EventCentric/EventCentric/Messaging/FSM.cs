using EventCentric.Messaging.Events;

namespace EventCentric.Messaging
{
    public abstract class FSM : Worker,
        IMessageHandler<FatalErrorOcurred>
    {
        protected volatile bool stopping;

        protected volatile bool systemHaltRequested = false;
        protected FatalErrorException fatalException = null;

        protected FSM(IBus bus)
            : base(bus)
        { }

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
