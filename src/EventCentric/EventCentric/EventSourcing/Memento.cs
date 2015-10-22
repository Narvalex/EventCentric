namespace EventCentric.EventSourcing
{
    public class Memento : IMemento
    {
        public Memento(long version)
        {
            this.Version = version;
        }

        public long Version { get; private set; }
    }
}
