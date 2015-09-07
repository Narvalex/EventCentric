using EventCentric.EventSourcing;
using System;

namespace EventCentric.Queueing
{
    public class QueuedEvent : Event
    {
        public QueuedEvent(Guid streamId, Guid transactionId)
        {
            this.StreamId = streamId;
            this.TransactionId = transactionId;
            this.StreamType = string.Empty;
            this.Version = default(int);
        }
    }
}
