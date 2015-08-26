namespace EventCentric.Transport
{
    public class NewEvent
    {
        public NewEvent(int eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public int EventCollectionVersion { get; set; }
        public string Payload { get; private set; }
    }
}
