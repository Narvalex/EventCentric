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

        public EventPollster(IBus bus, BufferPool buffer)
            : base(bus)
        {
            Ensure.NotNull(buffer, "buffer");

            this.buffer = buffer;
        }

        public void Handle(StopEventPollster message)
        {
            base.Stop();
        }

        public void Handle(StartEventPollster message)
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
