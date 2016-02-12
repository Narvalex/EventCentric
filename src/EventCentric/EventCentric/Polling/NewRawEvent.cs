namespace EventCentric.Transport
{
    public class NewRawEvent
    {
        public NewRawEvent(long eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public long EventCollectionVersion { get; set; }
        public string Payload { get; private set; }
    }
}
