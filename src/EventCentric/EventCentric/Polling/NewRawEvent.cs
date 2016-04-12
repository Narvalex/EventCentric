namespace EventCentric.Polling
{
    public class NewRawEvent
    {
        public NewRawEvent(long eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public long EventCollectionVersion { get; }
        public string Payload { get; }
    }
}
