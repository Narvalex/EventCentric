using System;

namespace EventCentric.EventSourcing
{
    public static class EventExtensions
    {
        public static Event AsIncomingMessage(this Event e, Guid transactionId, Guid streamId)
        {
            e.TransactionId = transactionId;
            e.StreamId = streamId;
            e.StreamType = string.Empty;
            e.Version = 0;
            return e;
        }

        public static IEvent AsIncomingMessage(this IEvent e, Guid transactionId, Guid streamId)
        {
            return ((Event)e).AsIncomingMessage(transactionId, streamId);
        }

        public static Event AsQueuedEvent(this Event e, string streamType, Guid eventId, int version)
        {
            e.StreamType = streamType;
            e.EventId = eventId;
            e.Version = version;
            return e;
        }

        public static IEvent AsQueuedEvent(this IEvent e, string streamType, Guid eventId, int version = 0)
        {
            return ((Event)e).AsQueuedEvent(streamType, eventId, version);
        }

        public static Event AsStoredEvent(this Event e, Guid transactionId, Guid eventId, string streamType)
        {
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamType = streamType;
            return e;
        }

        public static IEvent AsStoredEvent(this IEvent e, Guid transactionId, Guid eventId, string streamType)
        {
            return ((Event)e).AsStoredEvent(transactionId, eventId, streamType);
        }
    }
}
