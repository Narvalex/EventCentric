using System;

namespace EventCentric.EventSourcing
{
    public static class EventExtensions
    {
        public static Event FormatAsQueuedEvent(this Event e, Guid transactionId, Guid streamId)
        {
            e.TransactionId = transactionId;
            e.StreamId = streamId;
            e.StreamType = string.Empty;
            e.Version = 0;
            return e;
        }

        public static Event FormatAsInProcessEvent(this Event e, Guid transactionId, Guid eventId, Guid streamId, string streamType)
        {
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamId = streamId;
            e.StreamType = streamType;
            e.Version = 0;
            return e;
        }

        public static Event FormatAsStoredEvent(this Event e, Guid transactionId, Guid eventId, string streamType)
        {
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamType = streamType;
            return e;
        }

        public static IEvent FormatAsStoredEvent(this IEvent e, Guid transactionId, Guid eventId, string streamType)
        {
            return ((Event)e).FormatAsStoredEvent(transactionId, eventId, streamType);
        }
    }
}
