using EventCentric.EventSourcing;
using EventCentric.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventCentric.Tests.Serialization
{
    [TestClass]
    public class SerializationExceptionHandlingFixture
    {
        protected JsonTextSerializer sut = new JsonTextSerializer();

        [TestMethod]
        public void WHEN_unregistered_payload_arrives_THEN_can_deserialize_to_a_base_class()
        {
            var deserialized = this.sut.Deserialize<Event>("{\"$type\":\"EventCentric.Tests.Serialization.SampleEvent, EventCentric.Tests\",\"Payload\":\"a payload\",\"BasePayload\":\"a base payload\",\"EventCollectionVersion\":0,\"TransactionId\":\"00000000-0000-0000-0000-000000000000\",\"EventId\":\"00000000-0000-0000-0000-000000000000\",\"ProcessorBufferVersion\":0,\"StreamId\":\"00000000-0000-0000-0000-000000000000\",\"StreamType\":null,\"Version\":0,\"LocalTime\":\"0001-01-01T00:00:00\",\"UtcTime\":\"0001-01-01T00:00:00\"}");
        }
    }
}
