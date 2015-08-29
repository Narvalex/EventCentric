using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System.Threading;

namespace EventCentric
{
    public class Node : FSM, INode,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventProcessorStarted>,
        IMessageHandler<EventPollsterStarted>,
        IMessageHandler<EventPollsterStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        public Node(IBus bus)
            : base(bus)
        {
            this.State = NodeState.Down;
        }

        public NodeState State { get; private set; }

        /// <summary>
        /// Starts engine
        /// </summary>
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

        /// <summary>
        /// Stops engine
        /// </summary>
        public new void Stop()
        {
            base.Stop();
        }

        protected override void OnStarting()
        {
            this.bus.Publish(new StartEventPublisher());
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

                this.bus.Publish(new StopEventProcessor(), new StopEventPublisher(), new StopEventPollster());
                this.State = NodeState.Down;
                this.OnStopping();
            }
        }

        public void Handle(EventPollsterStarted message)
        {
            this.State = NodeState.UpAndRunning;
        }

        public void Handle(EventPublisherStarted message)
        {
            this.bus.Publish(new StartEventProcessor());
        }

        public void Handle(EventProcessorStarted message)
        {
            this.bus.Publish(new StartEventPollster());
        }

        public void Handle(EventPollsterStopped message)
        {
            //..
        }

        public void Handle(EventProcessorStopped message)
        {
            //...
        }

        public void Handle(EventPublisherStopped message)
        {
            //...
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
