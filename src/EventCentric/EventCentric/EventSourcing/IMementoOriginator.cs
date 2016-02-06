namespace EventCentric.EventSourcing
{
    public interface IMementoOriginator<T> where T : IMemento
    {
        T SaveToMemento();
    }

    public interface IMemento { }

    public class VoidMemento : IMemento
    { }
}
