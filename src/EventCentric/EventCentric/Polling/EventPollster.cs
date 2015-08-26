using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Polling;
using EventCentric.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Pulling
{
    public class EventPollster : FSM,
        IMessageHandler<StartEventPuller>,
        IMessageHandler<StopEventPuller>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly EventBuffer buffer;
        private readonly ProcessorProxy proxy;

        protected EventPollster(IBus bus, EventBuffer buffer, ProcessorProxy proxy)
            : base(bus)
        {
            Ensure.NotNull(buffer, "buffer");
            Ensure.NotNull(proxy, "proxy");

            this.buffer = buffer;
            this.proxy = proxy;
        }

        public void Handle(StopEventPuller message)
        {
            base.Stop();
        }

        public void Handle(StartEventPuller message)
        {
            base.Start();
        }

        protected override void OnStarting()
        {
            Task.Factory.StartNewLongRunning(() => this.Poll());

            // Ensure to start everything;
            this.bus.Publish(new EventPullerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPullerStopped());
        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            // TODO: log when poison was detected and signal to shut down everything.

            this.bus.Publish(
                new FatalErrorOcurred(
                    new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception)));
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            throw new NotImplementedException();
        }

        private void Poll()
        {
            this.buffer.Initialize();

            while (!base.stopping)
            {
                var areBusy = new Tuple<bool, bool>(
                    this.buffer.TryFill(),
                    this.proxy.TryPushEvents());

                if (!areBusy.Item1 && !areBusy.Item2)
                    Thread.Sleep(100);
            }
        }
    }
}
