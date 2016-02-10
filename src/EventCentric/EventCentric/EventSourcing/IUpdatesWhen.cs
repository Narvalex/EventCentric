namespace EventCentric.EventSourcing
{
    public interface IUpdatesWhen<T> where T : Event
    {
        void When(T e);
    }

    public interface IUpdatesWhenSent<T> where T : Command
    {
        void WhenSent(T c);
    }

    public interface IStreamWithExceptions : IUpdatesWhen<AnInvalidOperationExceptionOccurred>
    { }
}