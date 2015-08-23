namespace EventCentric.EventSourcing
{
    public interface IUpdatesOn { }

    public interface IUpdatesOn<T> : IUpdatesOn where T : IEvent
    {
        void On(T e);
    }
}
