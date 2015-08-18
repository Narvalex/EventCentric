namespace EventCentric.EventSourcing
{
    public interface IHandle { }

    public interface IHandle<T> : IHandle where T : IEvent
    {
        void Handle(T e);
    }
}
