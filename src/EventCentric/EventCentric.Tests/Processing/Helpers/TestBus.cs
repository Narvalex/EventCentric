using EventCentric.Messaging;
using System.Collections.Generic;

namespace EventCentric.Tests.Processing.Helpers
{
    public class TestBus : IBus
    {
        public List<IMessage> Messages { get; } = new List<IMessage>();

        public void Publish(params IMessage[] messages)
        {

        }

        public void Publish(IMessage message)
        {
            this.Messages.Add(message);
        }
    }
}
