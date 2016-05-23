using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class EventPollerStopped : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }

    public class EventProcessorStopped : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }

    public class EventPublisherStopped : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }
}
