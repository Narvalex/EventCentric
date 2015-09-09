namespace EventCentric.EventSourcing
{
    public class Memento : IMemento
    {
        public Memento(int version)
        {
            this.Version = version;
        }

        public int Version { get; private set; }
    }
}
