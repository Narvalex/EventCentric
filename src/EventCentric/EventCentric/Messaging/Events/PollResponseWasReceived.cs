using EventCentric.Transport;

namespace EventCentric.Messaging.Events
{
    public class PollResponseWasReceived : IMessage
    {
        public PollResponseWasReceived(PollResponse response)
        {
            this.Response = response;
        }

        public PollResponse Response { get; private set; }
    }
}
