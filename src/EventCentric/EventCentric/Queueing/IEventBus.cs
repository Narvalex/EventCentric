using EventCentric.EventSourcing;

namespace EventCentric.Queueing
{
    public interface IEventBus
    {
        void Send(IEvent @event);
    }
}
