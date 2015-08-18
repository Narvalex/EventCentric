namespace EventCentric.EventSourcing
{
    public interface IUpdateOn { }

    public interface IUpdateOn<T> : IUpdateOn where T : IEvent
    {
        void On(T e);
    }
}
