using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System.Threading;

namespace EventCentric
{
    public class EventProcessorMicroservice : MicroserviceBase, IMicroservice,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventProcessorStarted>,
        IMessageHandler<EventPollerStarted>,
        IMessageHandler<EventPollerStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        private bool hasPoller;
        private bool listenHeartbeating;

        public EventProcessorMicroservice(string eventSourceName, IBus bus, ILogger log, bool hasPoller, bool listenHeartbeating)
            : base(eventSourceName, bus, log)
        {
            this.State = WorkerStatus.Down;
            this.hasPoller = hasPoller;
            this.listenHeartbeating = listenHeartbeating;
        }

        public WorkerStatus State { get; private set; }

        /// <summary>
        /// Starts engine
        /// </summary>
        public new void Start()
        {
            if (systemHaltRequested)
                this.OnSystemHalt();

            // Is started
            if (this.State == WorkerStatus.UpAndRunning)
                return;

            if (this.State == WorkerStatus.Down)
            {
                this.State = WorkerStatus.Starting;
                this.log.Trace($"Starting node {this.Name}");
                base.Start();

                // Check if is started to release thread.
                this.Start();
            }

            if (this.State == WorkerStatus.Starting || this.State == WorkerStatus.ShuttingDown)
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
            if (this.State == WorkerStatus.Down)
                return;

            // Engine is sutting down, or is starting wait a little bit until is finalize its transitioning.
            if (this.State == WorkerStatus.ShuttingDown)
            {
                Thread.Sleep(100);
                this.OnStopping();
            }

            // This means that the System is up and running. Can safely stop
            if (this.State == WorkerStatus.UpAndRunning || this.State == WorkerStatus.Starting)
            {
                this.State = WorkerStatus.ShuttingDown;

                this.bus.Publish(new StopEventProcessor(), new StopEventPublisher(), new StopEventPoller());
                this.State = WorkerStatus.Down;
                this.OnStopping();
            }
        }

        public void Handle(EventPollerStarted message)
        {
            this.State = WorkerStatus.UpAndRunning;
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
                this.State = WorkerStatus.UpAndRunning;
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
                if (this.State == WorkerStatus.Down)
                    throw this.fatalException;
                else
                    Thread.Sleep(100);
            }
        }
    }
}
