using EventCentric.Transport;

namespace EventCentric.Publishing.Dto
{
    public class ClientData
    {
        public ClientData(string clientVersion, PollResponse pollResponse)
        {
            this.ClientVersion = clientVersion;
            this.PollResponse = pollResponse;
        }

        public string ClientVersion { get; }

        public PollResponse PollResponse { get; }
    }
}
