namespace EventCentric.EventSourcing
{
    public interface IHandles { }

    public interface IHandles<T> : IHandles where T : IEvent
    {
        void Handle(T message);
    }
}
