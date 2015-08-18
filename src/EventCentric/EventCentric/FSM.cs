using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System;
using System.Threading;

namespace EventCentric
{
    public class FSM : Worker, INode, IDisposable,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventProcessorStarted>,
        IMessageHandler<EventPullerStarted>,
        IMessageHandler<EventPullerStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        public enum NodeState
        {
            Down,
            ShuttingDown,
            Starting,
            UpAndRunning
        }

        public FSM(IBus bus)
            : base(bus)
        {
            this.State = NodeState.Down;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this.Stop();
        }

        public NodeState State { get; private set; }

        public void Handle(EventProcessorStarted message)
        {
            this.bus.Publish(new StartEventPuller());
        }
        /// <summary>
        /// Starts engine
        /// </summary>
        public new void Start()
        {
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
            // Engine is down
            if (this.State == NodeState.Down)
                return;

            // Engine is sutting down, or is starting wait a little bit until is finalize its transitioning.
            if (this.State == NodeState.ShuttingDown || this.State == NodeState.Starting)
            {
                Thread.Sleep(100);
                this.Stop();
            }

            // This means that the System is up and running. Can safely stop
            if (this.State == NodeState.UpAndRunning)
            {
                this.State = NodeState.ShuttingDown;
                base.Stop();

                // Call again to check if it stops to realease thread.
                this.Stop();
            }
        }

        protected override void OnStarting()
        {
            this.bus.Publish(new StartEventPublisher());
        }

        protected override void OnStopping()
        {
            this.bus.Publish(new StopEventPuller());
        }

        public void Handle(EventPullerStarted message)
        {
            this.State = NodeState.UpAndRunning;
        }

        public void Handle(EventPublisherStarted message)
        {
            this.bus.Publish(new StartEventProcessor());
        }

        public void Handle(EventPullerStopped message)
        {
            this.bus.Publish(new StopEventProcessor());
        }

        public void Handle(EventProcessorStopped message)
        {
            this.bus.Publish(new StopEventPublisher());
        }

        public void Handle(EventPublisherStopped message)
        {
            this.State = NodeState.Down;
        }
    }
}
