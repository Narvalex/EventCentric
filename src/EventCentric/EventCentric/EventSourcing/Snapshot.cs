namespace EventCentric.EventSourcing
{
    public class Snapshot : ISnapshot
    {
        public Snapshot(long version)
        {
            this.Version = version;
        }

        public long Version { get; private set; }
    }
}
