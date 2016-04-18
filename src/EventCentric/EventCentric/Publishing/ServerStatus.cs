namespace EventCentric.Publishing.Dto
{
    public class ServerStatus
    {
        public ServerStatus(long eventBufferVersion)
        {
            this.EventBufferVersion = eventBufferVersion;
        }

        public long EventBufferVersion { get; }
    }
}
