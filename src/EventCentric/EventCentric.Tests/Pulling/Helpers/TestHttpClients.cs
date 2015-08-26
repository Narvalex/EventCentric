using EventCentric.Serialization;
using EventCentric.Transport;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric.Tests.Pulling.Helpers
{
    public class TestHttpClientWithSingleResult : IOldHttpPoller
    {
        private readonly JsonTextSerializer serializer = new JsonTextSerializer();
        private Guid streamId;

        private bool NewEventToBeFound;

        public TestHttpClientWithSingleResult(Guid streamId, bool newEventToBeFound = true)
        {
            this.NewEventToBeFound = newEventToBeFound;
            this.streamId = streamId;
        }

        public OldPollEventsResponse PollEvents(string url)
        {
            Thread.Sleep(1000);

            var list = new List<OldPolledEventData>();

            if (this.NewEventToBeFound)
            {
                var payload = this.serializer.Serialize(new TestEvent1());
                list.Add(new OldPolledEventData("Clients", this.streamId, true, payload));
            }
            else
                list.Add(new OldPolledEventData("Clients", this.streamId, false, string.Empty));


            return new OldPollEventsResponse(true, list);
        }

        public PollStreamsResponse PollStreams(string url)
        {
            throw new NotImplementedException();
        }
    }
}
