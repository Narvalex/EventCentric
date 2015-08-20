using EventCentric.Messaging;
using System.Collections.Generic;

namespace EventCentric.Tests.Helpers
{
    public class GenericTestBus : IBus
    {
        public List<IMessage> Messages { get; set; } = new List<IMessage>();

        public void Publish(IMessage message)
        {
            this.Messages.Add(message);
        }
    }
}
