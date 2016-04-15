namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(string streamType, long eventCollectionVersion)
        {
            this.StreamType = streamType;
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public string StreamType { get; }
        public long EventCollectionVersion { get; }
    }
}
