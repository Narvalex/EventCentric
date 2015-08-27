namespace EventCentric.Transport
{
    public class NewRawEvent
    {
        public NewRawEvent(int eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public int EventCollectionVersion { get; set; }
        public string Payload { get; private set; }
    }
}
