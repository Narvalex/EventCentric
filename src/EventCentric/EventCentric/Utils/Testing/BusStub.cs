using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System.Collections.Generic;

namespace EventCentric.Utils.Testing
{
    //public class BusStub : IBus, IBusRegistry
    //{
    //    public readonly List<SystemMessage> Messages = new List<SystemMessage>();

    //    public void Publish(SystemMessage message)
    //    {
    //        this.Messages.Add(message);
    //        if (message is IncomingEventIsPoisoned)
    //        {
    //            var poisonedMessage = (IncomingEventIsPoisoned)message;
    //            throw new FatalErrorException($"Poisoned message detected in test", poisonedMessage.Exception);
    //        }

    //    }

    //    public void Register(ISystemHandler worker)
    //    { }
    //}
}
