using System.Threading;

namespace EventCentric.Messaging.Commands
{
    public class StartEventPublisher : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }

    public class StartEventProcessor : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }
    }

    public class StartEventPoller : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public StartEventPoller(string microserviceName)
        {
            this.MicroserviceName = microserviceName;
        }

        public string MicroserviceName { get; }
    }
}
