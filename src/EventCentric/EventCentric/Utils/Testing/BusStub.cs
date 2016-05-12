using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System.Collections.Generic;

namespace EventCentric.Utils.Testing
{
    public class BusStub : ISystemBus, IBusRegistry
    {
        public readonly List<IMessage> Messages = new List<IMessage>();

        public void Publish(IMessage message)
        {
            this.Messages.Add(message);
            if (message is IncomingEventIsPoisoned)
            {
                var poisonedMessage = (IncomingEventIsPoisoned)message;
                throw new FatalErrorException($"Poisoned message detected in test", poisonedMessage.Exception);
            }

        }

        public void Register(ISystemHandler worker)
        { }
    }
}
