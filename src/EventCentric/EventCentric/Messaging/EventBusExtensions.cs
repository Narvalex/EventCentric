using EventCentric.EventSourcing;
using System;

namespace EventCentric.Messaging
{
    public static class EventBusExtensions
    {
        public static void Send(this IEventBus bus, Guid transactionId, Guid streamId, IEvent command)
        {
            bus.Publish(transactionId, streamId, command);
        }
    }
}
