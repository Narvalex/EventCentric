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

            var list = new List<PolledEventData>();

            if (this.NewEventToBeFound)
            {
                var payload = this.serializer.Serialize(new TestEvent1());
                list.Add(new PolledEventData("Clients", this.streamId, true, payload));
            }
            else
                list.Add(new PolledEventData("Clients", this.streamId, false, string.Empty));


            var response = new PollResponse(list);
            return this.serializer.Serialize(response);
        }
    }
}
