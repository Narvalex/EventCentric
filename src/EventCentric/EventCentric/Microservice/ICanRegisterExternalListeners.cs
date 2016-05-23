using EventCentric.Messaging;
using System;

namespace EventCentric.Microservice
{
    public interface ICanRegisterExternalListeners
    {
        void Register(Action<IBus> externalRegistrationInLocalBus);
    }
}
