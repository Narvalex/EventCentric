namespace EventCentric.EventSourcing
{
    public interface IHandlerOf<T> where T : IEvent
    {
        void On(T e);
    }
}
