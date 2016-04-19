namespace EventCentric.Messaging.Events
{
    public struct EventStoreHasBeenUpdated : IMessage
    {
        public EventStoreHasBeenUpdated(long eventCollectionVersion)
        {
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public long EventCollectionVersion { get; }
    }
}