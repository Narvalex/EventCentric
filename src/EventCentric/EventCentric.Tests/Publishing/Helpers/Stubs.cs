using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Publishing;
using System.Collections.Generic;

namespace EventCentric.Tests.Publishing.Helpers
{
    public class BusStub : ISystemBus, IBusRegistry
    {
        public void Publish(IMessage message)
        {
        }

        public void Register(ISystemHandler worker)
        {

        }
    }

    public class EventDaoStub : IEventDao
    {
        public NewRawEvent[] FindEvents(long fromEventCollectionVersion, int quantity)
        {
            return new List<NewRawEvent>
            {
                new NewRawEvent(1, "serializedEventPayloadHere"),
                new NewRawEvent(2, "serializedEventPayloadHere"),
                new NewRawEvent(3, "serializedEventPayloadHere")
            }.ToArray();
        }

        public long GetEventCollectionVersion() => 3;
    }
}
