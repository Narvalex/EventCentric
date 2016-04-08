using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Publishing;
using EventCentric.Transport;
using System.Collections.Generic;

namespace EventCentric.Tests.Publishing.Helpers
{
    public class BusStub : IBus, IBusRegistry
    {
        public void Publish(IMessage message)
        {
        }

        public void Register(IWorker worker)
        {

        }
    }

    public class EventDaoStub : IEventDao
    {
        public List<NewRawEvent> FindEvents(long fromEventCollectionVersion, int quantity)
        {
            return new List<NewRawEvent>
            {
                new NewRawEvent(1, "serializedEventPayloadHere"),
                new NewRawEvent(2, "serializedEventPayloadHere"),
                new NewRawEvent(3, "serializedEventPayloadHere")
            };
        }

        public long GetEventCollectionVersion() => 3;
    }
}
