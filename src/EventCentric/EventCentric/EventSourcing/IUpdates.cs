namespace EventCentric.EventSourcing
{
    public interface IUpdatesWhen<T> where T : Event
    {
        void When(T e);
    }

    public interface IUpdatesAfterSending<T> where T : Command
    {
        void AfterSending(T c);
    }

    public interface IStreamWithExceptions : IUpdatesWhen<AnInvalidOperationExceptionOccurred>
    { }
}