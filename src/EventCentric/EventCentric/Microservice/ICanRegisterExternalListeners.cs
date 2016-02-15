using EventCentric.Messaging;

namespace EventCentric.Microservice
{
    public interface ICanRegisterExternalListeners
    {
        void Register(IWorker externalListener);
    }
}
