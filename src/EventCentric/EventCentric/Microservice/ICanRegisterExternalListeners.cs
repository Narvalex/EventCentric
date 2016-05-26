using EventCentric.Messaging;
using System;

namespace EventCentric
{
    public interface ICanRegisterExternalListeners
    {
        void Register(Action<IBus> externalRegistrationInLocalBus);
    }
}
