using EventCentric.Serialization;
using EventCentric.Transport;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestHttpClientWithSingleResult : IHttpClient
    {
        private readonly JsonTextSerializer serializer = new JsonTextSerializer();
        private Guid streamId;

        private bool NewEventToBeFound;

        public TestHttpClientWithSingleResult(Guid streamId, bool newEventToBeFound = true)
        {
            this.NewEventToBeFound = newEventToBeFound;
            this.streamId = streamId;
        }

        public void Dispose()
        { }

        public Task<string> GetStringAsync(string requestUri)
        {
            return Task<string>.Factory.StartNew(() => this.GetSerializedPayload());
        }

        private string GetSerializedPayload()
        {
            Thread.Sleep(1000);
            var payload = this.serializer.Serialize(new TestEvent1());
            var list = new List<EventData> { new EventData(this.NewEventToBeFound, "Clients", this.streamId, payload) };
            var response = new PollResponse(list);
            return this.serializer.Serialize(response);
        }
    }
}
