namespace EventCentric.EventSourcing
{
    public interface IUpdatesOn<T> where T : IEvent
    {
        void On(T e);
    }

    public interface IStreamWithExceptions : IUpdatesOn<AnInvalidOperationExceptionOccurred>
    { }
}