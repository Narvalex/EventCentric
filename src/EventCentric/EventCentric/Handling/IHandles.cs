using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    public interface IHandles<T> where T : IEvent
    {
        IMessageHandling Handle(T message);
    }

    public interface IExceptionHandler : IHandles<AnInvalidOperationExceptionOccurred>
    { }
}
