using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System.Threading;

namespace EventCentric
{
    public class ProcessorNode : NodeBase, INode,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventProcessorStarted>,
        IMessageHandler<EventPollerStarted>,
        IMessageHandler<EventPollerStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        private bool hasPoller;
        private bool listenHeartbeating;

        public ProcessorNode(string nodeName, IBus bus, ILogger log, bool hasPoller, bool listenHeartbeating)
            : base(nodeName, bus, log)
        {
            this.State = NodeState.Down;
            this.hasPoller = hasPoller;
            this.listenHeartbeating = listenHeartbeating;
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
                this.log.Trace($"Starting node {this.Name}");
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
            base.OnStarting();

            if (!this.hasPoller)
                this.log.Trace("No poller detected");

            if (this.listenHeartbeating)
                this.bus.Publish(new StartHeartbeatListener());

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

                this.bus.Publish(new StopEventProcessor(), new StopEventPublisher(), new StopEventPoller());
                this.State = NodeState.Down;
                this.OnStopping();
            }
        }

        public void Handle(EventPollerStarted message)
        {
            this.State = NodeState.UpAndRunning;
            this.log.Trace("All services are up and running");

        }

        public void Handle(EventPublisherStarted message)
        {
            this.bus.Publish(new StartEventProcessor());
        }

        public void Handle(EventProcessorStarted message)
        {
            if (this.hasPoller)
                this.bus.Publish(new StartEventPoller());
            else
            {
                this.State = NodeState.UpAndRunning;
                this.log.Trace("All services are up and running");
            }
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
