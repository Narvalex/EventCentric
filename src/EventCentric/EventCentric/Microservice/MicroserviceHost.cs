using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Microservice;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric
{
    public class MicroserviceHost : MicroserviceBase, IMicroservice, ICanRegisterExternalListeners,
        IMessageHandler<EventPublisherStarted>,
        IMessageHandler<EventHandlerStarted>,
        IMessageHandler<EventPollerStarted>,
        IMessageHandler<EventPollerStopped>,
        IMessageHandler<EventProcessorStopped>,
        IMessageHandler<EventPublisherStopped>
    {
        private bool hasPoller;

        public MicroserviceHost(string eventSourceName, IBus bus, ILogger log, bool hasPoller)
            : base(eventSourceName, bus, log)
        {
            this.Status = WorkerStatus.Down;
            this.hasPoller = hasPoller;
        }

        public WorkerStatus Status { get; private set; }

        /// <summary>
        /// Starts engine
        /// </summary>
        public new void Start()
        {
            if (systemHaltRequested)
                this.OnSystemHalt();

            // Is started
            if (this.Status == WorkerStatus.UpAndRunning)
                return;

            if (this.Status == WorkerStatus.Down)
            {
                this.Status = WorkerStatus.Starting;
                base.Start();

                // Check if is started to release thread.
                this.Start();
            }

            if (this.Status == WorkerStatus.Starting || this.Status == WorkerStatus.ShuttingDown)
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
                this.log.Log("No poller detected");

            this.bus.Publish(new StartEventPublisher());
        }

        protected override void OnStopping()
        {
            // Engine is down
            if (this.Status == WorkerStatus.Down)
                return;

            // Engine is sutting down, or is starting wait a little bit until is finalize its transitioning.
            if (this.Status == WorkerStatus.ShuttingDown)
            {
                Thread.Sleep(100);
                this.OnStopping();
            }

            // This means that the System is up and running. Can safely stop
            if (this.Status == WorkerStatus.UpAndRunning || this.Status == WorkerStatus.Starting)
            {
                this.Status = WorkerStatus.ShuttingDown;

                this.bus.Publish(new StopEventProcessor());
                this.bus.Publish(new StopEventPublisher());
                this.bus.Publish(new StopEventPoller());
                this.Status = WorkerStatus.Down;
                this.OnStopping();
            }
        }

        public void Register(ISystemHandler externalListener) =>
           ((IBusRegistry)base.bus).Register(externalListener);

        public void Handle(EventPollerStarted message)
        {
            this.Status = WorkerStatus.UpAndRunning;

            var lines = new List<string>();
            lines.Add("-----------------------------------------------------------------------------------");
            lines.Add($"Microservice {this.Name} is up and running");
            lines.Add("-----------------------------------------------------------------------------------");

            this.log.Log("", lines.ToArray());
        }

        public void Handle(EventPublisherStarted message)
        {
            this.bus.Publish(new StartEventProcessor());
        }

        public void Handle(EventHandlerStarted message)
        {
            if (this.hasPoller)
                this.bus.Publish(new StartEventPoller(this.Name));
            else
            {
                this.Status = WorkerStatus.UpAndRunning;
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
                if (this.Status == WorkerStatus.Down)
                    throw this.fatalException;
                else
                    Thread.Sleep(100);
            }
        }
    }
}
