using EventCentric.EventSourcing;

namespace EventCentric.Processing
{
    public interface IEventHandler
    { }

    public interface IEventHandler<T> where T : IEvent
    {
        void Handle(T incomingEvent);
    }
}
