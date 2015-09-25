using EventCentric.EventSourcing;

namespace EventCentric.Queueing
{
    public interface IEventQueue
    {
        void Enqueue(IEvent @event);
    }
}
