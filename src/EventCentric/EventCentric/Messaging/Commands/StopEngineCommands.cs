using System.Threading;

namespace EventCentric.Messaging.Commands
{
    public class StopEventPoller : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public StopEventPoller(bool stopSilently = false)
        {
            this.StopSilently = stopSilently;
        }

        public bool StopSilently { get; }
    }

    public class StopEventHandler : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }

    public class StopEventPublisher : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }
}
