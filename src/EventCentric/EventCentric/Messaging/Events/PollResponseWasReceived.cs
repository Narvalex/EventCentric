using EventCentric.Transport;

namespace EventCentric.Messaging.Events
{
    public struct PollResponseWasReceived : IMessage
    {
        public PollResponseWasReceived(PollResponse response)
        {
            this.Response = response;
        }

        public PollResponse Response { get; }
    }
}
