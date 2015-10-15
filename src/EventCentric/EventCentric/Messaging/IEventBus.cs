using EventCentric.EventSourcing;

namespace EventCentric.Messaging
{
    public interface IEventBus
    {
        void Publish(IEvent @event);
    }
}
