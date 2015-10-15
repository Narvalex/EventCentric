namespace EventCentric.Messaging.Events
{
    public class IncomingEventHasBeenProcessed : IMessage
    {
        public IncomingEventHasBeenProcessed(string streamType, int eventCollectionVersion)
        {
            this.StreamType = streamType;
            this.EventCollectionVersion = eventCollectionVersion;
        }

        public string StreamType { get; private set; }
        public int EventCollectionVersion { get; private set; }
    }
}