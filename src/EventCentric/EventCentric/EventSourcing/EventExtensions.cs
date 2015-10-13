using System;

namespace EventCentric.EventSourcing
{
    public static class EventExtensions
    {
        public static Event AsQueueFormattedEvent(this Event e, Guid transactionId, Guid streamId)
        {
            e.TransactionId = transactionId;
            e.StreamId = streamId;
            e.StreamType = string.Empty;
            e.Version = 0;
            return e;
        }

        public static Event AsInProcessFormattedEvent(this Event e, Guid transactionId, Guid eventId, Guid streamId, string streamType)
        {
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamId = streamId;
            e.StreamType = streamType;
            e.Version = 0;
            return e;
        }

        public static Event AsStoreFormattedEvent(this Event e, Guid transactionId, Guid eventId, string streamType)
        {
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamType = streamType;
            return e;
        }

        public static IEvent AsStoreFormattedEvent(this IEvent e, Guid transactionId, Guid eventId, string streamType)
        {
            return ((Event)e).AsStoreFormattedEvent(transactionId, eventId, streamType);
        }
    }
}
