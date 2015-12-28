using EventCentric.EventSourcing;

namespace EventCentric.Processing
{
    public interface IHandlerOf<T> where T : IEvent
    {
        void Receive(T message);
    }
}
