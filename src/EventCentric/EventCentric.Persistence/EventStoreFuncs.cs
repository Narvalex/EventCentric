using EventCentric.Handling;
using EventCentric.Serialization;
using System;

namespace EventCentric.EventSourcing
{
    public class EventStoreFuncs
    {
        public static bool DefaultFilter(string consumer, ITextSerializer serializer, string payload) => true;

        public static SerializedEvent ApplyConsumerFilter(SerializedEvent e, string consumer, ITextSerializer serializer, Func<string, ITextSerializer, string, bool> filter)
        {
            if (filter(consumer, serializer, e.Payload))
                return e;

            var originalEvent = serializer.Deserialize<IEvent>(e.Payload);
            var cloaked = new CloakedEvent(originalEvent.EventCollectionVersion, originalEvent.StreamType);
            return new SerializedEvent(e.EventCollectionVersion, serializer.Serialize(cloaked));
        }
    }
}
