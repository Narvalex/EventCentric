using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Messaging
{
    public static class EventBusExtensions
    {
        public static void Send(this IEventBus bus, Guid transactionId, Guid streamId, IEvent command)
        {
            bus.Publish(transactionId, streamId, command);
        }

        public static void Send<T>(this ICrudEventBus bus, Guid transactionId, Guid streamId, IEvent command, Action<T> performCrudOperation)
            where T : IEventQueueDbContext
        {
            bus.Publish<T>(transactionId, streamId, command, performCrudOperation);
        }
    }
}
