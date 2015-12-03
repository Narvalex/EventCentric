using EventCentric.EventSourcing;
using System;

namespace EventCentric.Utils.Testing
{
    public static class EventExtensions
    {
        public static TEvent AsEventWithFixedTransactionId<TEvent>(this TEvent e, Guid fixedTransactionId)
            where TEvent : Event
        {
            e.TransactionId = fixedTransactionId;
            return e;
        }

        public static TEvent AsEventWithFixedStreamId<TEvent>(this TEvent e, Guid fixedStreamId)
            where TEvent : Event
        {
            e.StreamId = fixedStreamId;
            return e;
        }
    }
}
