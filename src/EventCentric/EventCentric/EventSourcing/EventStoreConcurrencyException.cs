using System;
using System.Runtime.Serialization;

namespace EventCentric.EventSourcing
{
    [Serializable]
    public class EventStoreConcurrencyException : Exception
    {
        public EventStoreConcurrencyException() { }

        public EventStoreConcurrencyException(string message) : base(message) { }

        public EventStoreConcurrencyException(string message, Exception inner) : base(message, inner) { }

        protected EventStoreConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
