using EventCentric.EventSourcing;
using EventCentric.Transport;

namespace EventCentric.Processing
{
    public interface IEventHandler
    { }

    public interface IEventHandler<T> where T : IncomingEvent<IEvent>
    {
        void Handle(T incomingEvent);
    }
}
