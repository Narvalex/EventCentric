using EventCentric.EventSourcing;
using EventCentric.Processing;

namespace EventCentric.Messaging
{
    public interface IClientBus
    {
        void Send(ICommand command);
        void Publish(IEvent @event);
    }
}
