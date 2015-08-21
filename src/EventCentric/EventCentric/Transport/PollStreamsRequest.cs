namespace EventCentric.Transport
{
    public class PollStreamsRequest
    {
        public PollStreamsRequest(int streamCollectionVersion)
        {
            this.StreamCollectionVersion = streamCollectionVersion;
        }

        public int StreamCollectionVersion { get; private set; }
    }
}
