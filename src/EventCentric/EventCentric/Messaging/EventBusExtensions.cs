using EventCentric.EventSourcing;
using EventCentric.Repository;
using System;

namespace EventCentric.Messaging
{
    public static class EventBusExtensions
    {
        public static void Send(this IEventBus bus, IEvent command)
        {
            bus.Publish(command);
        }

        public static void Send<T>(this ICrudEventBus bus, IEvent command, Action<T> performCrudOperation)
            where T : IEventQueueDbContext
        {
            bus.Publish<T>(command, performCrudOperation);
        }
    }
}
