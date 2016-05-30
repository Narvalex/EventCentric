using EventCentric.Handling;
using EventCentric.Serialization;
using System;

namespace EventCentric.EventSourcing
{
    public class EventStore
    {
        public static bool DefaultFilter(string consumer, string payload) => true;

        public static SerializedEvent ApplyConsumerFilter(SerializedEvent e, string consumer, ITextSerializer serializer, Func<string, string, bool> filter)
        {
            if (filter(consumer, e.Payload))
                return e;

            var originalEvent = serializer.Deserialize<IEvent>(e.Payload);
            var cloaked = new CloakedEvent()
            {
                TransactionId = originalEvent.TransactionId,
                EventId = originalEvent.EventId,
                StreamType = originalEvent.StreamType,
                StreamId = originalEvent.StreamId,
                Version = originalEvent.Version,
                EventCollectionVersion = originalEvent.EventCollectionVersion,
                ProcessorBufferVersion = originalEvent.ProcessorBufferVersion,
                LocalTime = originalEvent.LocalTime,
                UtcTime = originalEvent.UtcTime
            };
            return new SerializedEvent(e.EventCollectionVersion, serializer.Serialize(cloaked));
        }
    }
}
