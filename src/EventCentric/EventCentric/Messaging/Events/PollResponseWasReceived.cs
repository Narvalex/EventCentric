using EventCentric.Transport;
using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class PollResponseWasReceived : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public PollResponseWasReceived(PollResponse response)
        {
            this.Response = response;
        }

        public PollResponse Response { get; }
    }
}
