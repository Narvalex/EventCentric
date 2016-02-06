using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    public interface IHandles { }

    public interface IHandles<T> : IHandles where T : IEvent
    {
        void Handle(T message);
    }

    public interface IExceptionHandler : IHandles<AnInvalidOperationExceptionOccurred>
    { }
}
