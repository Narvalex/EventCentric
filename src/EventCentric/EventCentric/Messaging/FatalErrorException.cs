using System;
using System.Runtime.Serialization;

namespace EventCentric.Messaging
{
    [Serializable]
    public class FatalErrorException : Exception
    {
        public FatalErrorException() { }

        public FatalErrorException(string message)
            : base(message)
        { }

        public FatalErrorException(string message, Exception inner)
            : base(message, inner)
        { }

        public FatalErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
