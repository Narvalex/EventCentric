using EventCentric.EventSourcing;
using System;

namespace EventCentric
{
    public static class EventExtensions
    {
        /// <summary>
        /// Sets a message as idempotent.
        /// </summary>
        /// <param name="idempotencyId"></param>
        /// <returns></returns>
        public static T WithIdempotency<T>(this T message, Guid idempotencyId) where T : Message
        {
            message.EventId = idempotencyId;
            message.TransactionId = idempotencyId;
            return message;
        }
    }
}
