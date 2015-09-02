using EventCentric.Log;
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
        IMessageHandler<StartEventPollster>,
        IMessageHandler<StopEventPollster>
    {
        private readonly BufferPool buffer;

        public EventPollster(IBus bus, ILogger log, BufferPool buffer)
            : base(bus, log)
        {
            Ensure.NotNull(buffer, "buffer");

            this.buffer = buffer;
        }

        public void Handle(StopEventPollster message)
        {
            this.log.Trace("Stopping pollster");
            base.Stop();
            this.log.Trace("Pollster stopped");
        }

        public void Handle(StartEventPollster message)
        {
            this.log.Trace("Starting pollster");
            base.Start();
            this.log.Trace("Pollster started");
        }

        protected override void OnStarting()
        {
            Task.Factory.StartNewLongRunning(() => this.Poll());

            // Ensure to start everything;
            this.bus.Publish(new EventPollsterStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPollsterStopped());
        }

        private void Poll()
        {
            this.buffer.Initialize();

            while (!base.stopping)
            {
                var isStarving = new Tuple<bool, bool>(
                    !this.buffer.TryFill(),
                    !this.buffer.TryFlush());

                if (isStarving.Item1 && isStarving.Item2)
                    Thread.Sleep(100);
            }
        }
    }
}
