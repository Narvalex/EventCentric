namespace EventCentric.Messaging.Events
{
    public class StreamHasBeenUpdated : IMessage
    {
        public StreamHasBeenUpdated(int eventCollectionVersion)
        {
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public int EventCollectionVersion { get; private set; }
    }
}
