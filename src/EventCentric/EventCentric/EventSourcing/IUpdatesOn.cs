namespace EventCentric.EventSourcing
{
    public interface IUpdatesOn<T> where T : IEvent
    {
        void On(T e);
    }
}
