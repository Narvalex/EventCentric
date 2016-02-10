using EventCentric.EventSourcing;
using EventCentric.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventCentric.Tests.Serialization
{
    [TestClass]
    public class MessageSerializationFixture
    {
        protected JsonTextSerializer sut = new JsonTextSerializer();

        [TestMethod]
        public void Can_serialize_raw_events_and_commands()
        {
            var rawEvent = new Event();
            Assert.IsFalse(rawEvent.IsACommand);

            var rawCommand = new Command();
            Assert.IsTrue(rawCommand.IsACommand);

            var serializedEvent = this.sut.Serialize(rawEvent);
            var deserializedEvent = this.sut.Deserialize<IEvent>(serializedEvent);
            Assert.IsFalse(deserializedEvent.IsACommand);

            var serializedCommand = this.sut.Serialize(rawCommand);
            var deserializedCommand = this.sut.Deserialize<IEvent>(serializedCommand);
            Assert.IsTrue(deserializedCommand.IsACommand);
        }

        [TestMethod]
        public void Can_serialize_complex_events_and_commands()
        {
            var e = new EventA(1, "event");
            var deserializedEvent = this.sut.Deserialize<IEvent>(this.sut.Serialize(e));
            Assert.IsTrue(((EventA)deserializedEvent).Quantity == 1);
            Assert.IsTrue(((EventA)deserializedEvent).Text == "event");
            Assert.IsFalse(deserializedEvent.IsACommand);

            var c = this.sut.Deserialize<IEvent>(this.sut.Serialize(new CommandA(10, "command")));
            Assert.IsTrue(c.IsACommand);

            var fullCommand = (CommandA)c;
            Assert.AreEqual(10, fullCommand.Quantity);
            Assert.AreEqual("command", fullCommand.Text);
        }
    }

    public class CommandA : Command
    {
        public CommandA(int quantity, string text)
        {
            this.Quantity = quantity;
            this.Text = text;
        }

        public int Quantity { get; }
        public string Text { get; }
    }

    public class EventA : Event
    {
        public EventA(int quantity, string text)
        {
            this.Quantity = quantity;
            this.Text = text;
        }

        public int Quantity { get; }
        public string Text { get; }
    }
}
