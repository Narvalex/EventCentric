using EventCentric.Messaging;
using System;

namespace EventCentric.Tests.FiniteStateMachine.Helpers
{
    public class FakeBus : IBus
    {
        public void Publish(params IMessage[] messages)
        {
            throw new NotImplementedException();
        }

        public void Publish(IMessage message)
        {

        }
    }
}
