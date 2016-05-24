using System.Threading;

namespace EventCentric.Messaging.Commands
{
    /// <summary>
    /// This adds a new subscription on the fly and starts polling. If the subscription fails the 
    /// system will shutdown. The <see cref="EventCentric.Polling.ISubscriptionRepository"/> 
    /// will persist the subscription.
    /// </summary>
    public class AddNewSubscriptionOnTheFly : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public AddNewSubscriptionOnTheFly(string streamType, string url, string token)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.Token = token;
        }

        public string StreamType { get; }
        public string Url { get; }
        public string Token { get; }
    }
}
