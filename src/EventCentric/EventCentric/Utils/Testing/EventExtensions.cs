using EventCentric.EventSourcing;
using System;

namespace EventCentric.Utils.Testing
{
    public static class EventExtensions
    {
        public static TEvent AsVersion<TEvent>(this TEvent e, int version)
            where TEvent : Event
        {
            e.Version = version;
            return e;
        }

        public static TEvent WithStreamIdOf<TEvent>(this TEvent e, Guid streamId)
            where TEvent : Event
        {
            e.StreamId = streamId;
            return e;
        }
    }
}
