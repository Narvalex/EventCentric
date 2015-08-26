using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventCentric.Tests.Helpers
{
    public class TestLazyWorkersContainer :
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StartEventPollster>,
        IMessageHandler<StopEventPollster>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<StopEventPublisher>
    {
        private readonly IBus bus;

        public bool ThrowErrorOnStartup { get; set; } = false;
        public bool PublisherIsRunning { get; private set; }
        public bool ProcessorIsRunning { get; private set; }
        public bool PullerIsRunning { get; private set; }

        public IMessage NextMessage { get; private set; }

        public TestLazyWorkersContainer(IBus bus)
        {
            this.bus = bus;
            this.NextMessage = null;
            this.PublisherIsRunning = false;
            this.ProcessorIsRunning = false;
            this.PullerIsRunning = false;
        }

        public void Continue()
        {
            this.bus.Publish(this.NextMessage);
        }

        // Starting
        public void Handle(StartEventPublisher message)
        {
            Assert.IsFalse(this.PublisherIsRunning);
            Assert.IsFalse(this.ProcessorIsRunning);
            Assert.IsFalse(this.PullerIsRunning);

            this.PublisherIsRunning = true;

            if (this.ThrowErrorOnStartup)
                this.bus.Publish(new FatalErrorOcurred(new FatalErrorException()));
            else
                this.NextMessage = new EventPublisherStarted();
        }

        public void Handle(StartEventProcessor message)
        {
            Assert.IsTrue(this.PublisherIsRunning);
            Assert.IsFalse(this.ProcessorIsRunning);
            Assert.IsFalse(this.PullerIsRunning);

            this.ProcessorIsRunning = true;
            this.NextMessage = new EventProcessorStarted();
        }

        public void Handle(StartEventPollster message)
        {
            Assert.IsTrue(this.PublisherIsRunning);
            Assert.IsTrue(this.ProcessorIsRunning);
            Assert.IsFalse(this.PullerIsRunning);

            this.PullerIsRunning = true;
            this.NextMessage = new EventPullerStarted();
        }

        // Stopping
        public void Handle(StopEventPollster message)
        {
            Assert.IsTrue(this.PublisherIsRunning);
            Assert.IsTrue(this.ProcessorIsRunning);
            Assert.IsTrue(this.PullerIsRunning);

            this.PullerIsRunning = false;
            this.NextMessage = new EventPullerStopped();
        }

        public void Handle(StopEventProcessor message)
        {
            Assert.IsTrue(this.PublisherIsRunning);
            Assert.IsTrue(this.ProcessorIsRunning);
            Assert.IsFalse(this.PullerIsRunning);

            this.ProcessorIsRunning = false;
            this.NextMessage = new EventProcessorStopped();
        }

        public void Handle(StopEventPublisher message)
        {
            Assert.IsTrue(this.PublisherIsRunning);
            Assert.IsFalse(this.ProcessorIsRunning);
            Assert.IsFalse(this.PullerIsRunning);

            this.PublisherIsRunning = false;
            this.NextMessage = new EventPublisherStopped();
        }
    }
}
