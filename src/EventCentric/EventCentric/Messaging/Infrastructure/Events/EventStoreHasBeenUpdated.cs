namespace EventCentric.Messaging.Events
{
    public class EventStoreHasBeenUpdated : IMessage
    {
        public EventStoreHasBeenUpdated(long eventCollectionVersion)
        {
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public long EventCollectionVersion { get; private set; }
    }
}