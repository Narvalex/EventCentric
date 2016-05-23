using System.Threading;

namespace EventCentric.Messaging.Events
{
    public class FatalErrorOcurred : SystemMessage
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public override int MessageTypeId { get { return TypeId; } }

        public FatalErrorOcurred(FatalErrorException exception)
        {
            this.Exception = exception;
        }

        public FatalErrorException Exception { get; }
    }
}
