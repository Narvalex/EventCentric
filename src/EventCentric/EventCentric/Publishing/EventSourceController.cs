using EventCentric.Transport;
using System.Web.Http;

namespace EventCentric.Publishing
{
    public class EventSourceController : ApiController
    {
        private readonly IEventSource source;

        public EventSourceController(IEventSource source)
        {
            // We do not check for null for performance reasons.
            //Ensure.NotNull(source, "source");

            this.source = source;
        }

        [HttpGet]
        [Route("eventsource/events/{id1}/{v1}/{id2}/{v2}/{id3}/{v3}/{id4}/{v4}/{id5}/{v5}")]
        public IHttpActionResult Events(string id1, int v1, string id2, int v2, string id3, int v3, string id4, int v4, string id5, int v5)
        {
            var request = new PollEventsRequest(id1, v1, id2, v2, id3, v3, id4, v4, id5, v5);
            var response = this.source.PollEvents(request);

            return this.Ok(response);
        }

        [HttpGet]
        [Route("eventsource/streams/{streamCollectionVersion}")]
        public IHttpActionResult Streams(int streamCollectionVersion)
        {
            var request = new PollStreamsRequest(streamCollectionVersion);
            var response = this.source.PollStreams(request);

            return this.Ok(response);
        }

    }
}
