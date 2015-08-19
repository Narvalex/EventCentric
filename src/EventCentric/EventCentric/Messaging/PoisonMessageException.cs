using System;
using System.Runtime.Serialization;

namespace EventCentric.Messaging
{
    [Serializable]
    public class PoisonMessageException : Exception
    {
        public PoisonMessageException() { }

        public PoisonMessageException(string message)
            : base(message)
        { }

        public PoisonMessageException(string message, Exception inner)
            : base(message, inner)
        { }

        public PoisonMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
