using EventCentric.EventSourcing;

namespace EventCentric.Handling
{
    public interface IHandle<T> where T : IEvent
    {
        IMessageHandling Handle(T message);
    }

    public interface IExceptionHandler : IHandle<AnInvalidOperationExceptionOccurred>
    { }
}
