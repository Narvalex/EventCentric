using System;

namespace EventCentric.EventSourcing
{
    public static class EventExtensions
    {
        // And incoming message does not belong to a stream. Therefore, the stream type is null, and the 
        // event store will notice, and will not try to update any subscription status.
        public static IEvent AsIncomingMessage(this IEvent message, Guid transactionId, Guid streamId)
        {
            var e = (Event)message;
            e.TransactionId = transactionId;
            e.StreamId = streamId;
            e.StreamType = string.Empty;
            e.Version = 0;
            return e;
        }

        public static IEvent AsQueuedEvent(this IEvent @event, string streamType, Guid eventId, long version, DateTime timestamp)
        {
            var e = (Event)@event;
            e.StreamType = streamType;
            e.EventId = eventId;
            e.Version = version;
            e.Timestamp = timestamp;
            return e;
        }

        public static IEvent AsStoredEvent(this IEvent @event, Guid transactionId, Guid eventId, string streamType, DateTime timestamp)
        {
            var e = (Event)@event;
            e.TransactionId = transactionId;
            e.EventId = eventId;
            e.StreamType = streamType;
            e.Timestamp = timestamp;
            return e;
        }
    }
}
