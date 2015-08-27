namespace EventCentric.Messaging.Events
{
    public class EventStoreHasBeenUpdated : IMessage
    {
        public EventStoreHasBeenUpdated(int eventCollectionVersion)
        {
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public int EventCollectionVersion { get; private set; }
    }
}
