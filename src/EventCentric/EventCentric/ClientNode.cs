using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System.Threading;

namespace EventCentric
{
    public class ClientNode : FSM, INode,
        IMessageHandler<EventPublisherStarted>
    {
        public NodeState State { get; private set; }

        public ClientNode(IBus bus, ILogger log)
                : base(bus, log)
        { }

        public void Handle(EventPublisherStarted message)
        {
            //... up and running;
        }

        public new void Start()
        {
            if (systemHaltRequested)
                this.OnSystemHalt();

            // Is started
            if (this.State == NodeState.UpAndRunning)
                return;

            if (this.State == NodeState.Down)
            {
                this.State = NodeState.Starting;
                base.Start();

                // Check if is started to release thread.
                this.Start();
            }

            if (this.State == NodeState.Starting || this.State == NodeState.ShuttingDown)
            {
                Thread.Sleep(100);
                this.Start();
            }
        }

        public new void Stop()
        {
            base.Stop();
        }

        protected override void OnStarting()
        {
            this.bus.Publish(new StartEventPublisher());

            // we can safely go from here...
            this.State = NodeState.UpAndRunning;
        }

        protected override void OnStopping()
        {
            // Engine is down
            if (this.State == NodeState.Down)
                return;

            // Engine is sutting down, or is starting wait a little bit until is finalize its transitioning.
            if (this.State == NodeState.ShuttingDown)
            {
                Thread.Sleep(100);
                this.OnStopping();
            }

            // This means that the System is up and running. Can safely stop
            if (this.State == NodeState.UpAndRunning || this.State == NodeState.Starting)
            {
                this.State = NodeState.ShuttingDown;

                this.bus.Publish(new StopEventPublisher());
                this.State = NodeState.Down;
                this.OnStopping();
            }
        }

        private void OnSystemHalt()
        {
            while (true)
            {
                if (this.State == NodeState.Down)
                    throw this.fatalException;
                else
                    Thread.Sleep(100);
            }
        }
    }
}
