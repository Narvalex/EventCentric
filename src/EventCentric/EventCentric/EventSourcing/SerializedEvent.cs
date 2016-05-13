namespace EventCentric.EventSourcing
{
    public class SerializedEvent
    {
        public SerializedEvent(long eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public long EventCollectionVersion { get; }
        public string Payload { get; }
    }
}
