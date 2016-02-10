using EventCentric.EventSourcing;
using System;

namespace EventCentric.Utils.Testing
{
    public static class EventExtensions
    {
        public static TEvent AsEventWithFixedTransactionId<TEvent>(this TEvent e, Guid fixedTransactionId)
            where TEvent : Message
        {
            e.TransactionId = fixedTransactionId;
            return e;
        }

        public static IEvent AsEventWithFixedTransactionId(this IEvent e, Guid fixedTransactionId)
        {
            ((dynamic)e).TransactionId = fixedTransactionId;
            return e;
        }

        public static TEvent AsEventWithFixedStreamId<TEvent>(this TEvent e, Guid fixedStreamId)
            where TEvent : Message
        {
            e.StreamId = fixedStreamId;
            return e;
        }

        public static IEvent AsEventWithFixedStreamId(this IEvent e, Guid fixedStreamId)
        {
            ((dynamic)e).StreamId = fixedStreamId;
            return e;
        }
    }
}
