using EventCentric.EventSourcing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventCentric.Tests.Querying
{
    [TestClass]
    public class DynamicBindingFixture
    {
        protected ProcessorStub sut = new ProcessorStub();

        [TestMethod]
        public void WHEN_invoking_registered_message_THEN_invokes_the_correct_overload()
        {
            this.sut.Handle(new EventA());
            this.sut.Handle(new EventB());

            Assert.IsTrue(this.sut.EventAWasProcessed);
            Assert.IsTrue(this.sut.EventBWasProcessed);
            Assert.AreEqual(0, this.sut.DefaultEventProcessedCount);
        }

        [TestMethod]
        public void WHEN_invoking_non_registered_messages_THEN_invokes_default_overload()
        {
            this.sut.Handle(new EventC());
            this.sut.Handle(new EventD("something"));

            Assert.IsFalse(this.sut.EventAWasProcessed);
            Assert.IsFalse(this.sut.EventBWasProcessed);
            Assert.AreEqual(2, this.sut.DefaultEventProcessedCount);
        }
    }

    public class EventA : Event { }
    public class EventB : Event { }
    public class EventC : Event { }
    public class EventD : Event
    {
        public EventD(string payload)
        {
            this.Payload = payload;
        }
        public string Payload { get; }
    }

    public class ProcessorStub
    {
        public bool EventAWasProcessed { get; private set; } = false;
        public bool EventBWasProcessed { get; private set; } = false;
        public int DefaultEventProcessedCount { get; private set; } = 0;

        public void Handle(IEvent e)
        {
            ((dynamic)this).On((dynamic)e);
        }

        public void On(EventA e)
        {
            this.EventAWasProcessed = true;
        }

        public void On(EventB e)
        {
            this.EventBWasProcessed = true;
        }

        public void On(Event e)
        {
            ++this.DefaultEventProcessedCount;
        }
    }
}
