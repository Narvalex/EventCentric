using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System.Threading;

namespace EventCentric
{
    public class SagaNode : NodeBase, INode,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventProcessorStarted>,
        IMessageHandler<EventPollerStarted>,
        IMessageHandler<EventPollerStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        public SagaNode(string nodeName, IBus bus, ILogger log)
            : base(nodeName, bus, log)
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
                this.log.Trace("Starting node");
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

            // No user can issue a request, the pollster will receive new events after the publisher is 
            // up and running, so we can say that the node is up and running.
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

                this.bus.Publish(new StopEventProcessor(), new StopEventPublisher(), new StopEventPoller());
                this.State = NodeState.Down;
                this.OnStopping();
            }
        }

        public void Handle(EventPollerStarted message)
        {
            //this.State = NodeState.UpAndRunning;
            this.log.Trace("All services are up and running");
        }

        public void Handle(EventPublisherStarted message)
        {
            this.bus.Publish(new StartEventProcessor());
        }

        public void Handle(EventProcessorStarted message)
        {
            this.bus.Publish(new StartEventPoller());
        }

        public void Handle(EventPollerStopped message)
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
