using EventCentric.Messaging;
using System.Collections.Generic;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestBus : IBus
    {
        public List<IMessage> Messages { get; set; } = new List<IMessage>();

        public void Publish(IMessage message)
        {
            this.Messages.Add(message);
        }
    }
}
