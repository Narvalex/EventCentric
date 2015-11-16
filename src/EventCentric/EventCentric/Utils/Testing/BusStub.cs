using EventCentric.Messaging;
using System.Collections.Generic;

namespace EventCentric.Utils.Testing
{
    public class BusStub : IBus, IBusRegistry
    {
        public readonly List<IMessage> Messages = new List<IMessage>();

        public void Publish(IMessage message)
            => this.Messages.Add(message);

        public void Register(IWorker worker)
        { }
    }
}
