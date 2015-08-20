using EventCentric.EventSourcing;

namespace EventCentric.Messaging
{
    public interface IEventBus
    {
        void Send(IEvent @event);
    }
}
