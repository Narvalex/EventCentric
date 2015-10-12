using EventCentric.EventSourcing;

namespace EventCentric.Processing
{
    public interface ISubscribedTo
    { }

    public interface ISubscribedTo<T> where T : IEvent
    {
        void Receive(T message);
    }
}
