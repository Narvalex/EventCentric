using EventCentric.Transport;
using System.Web.Http;

namespace EventCentric.Publishing
{
    public class StreamsController : ApiController
    {
        private readonly IEventSource source;

        public StreamsController(IEventSource source)
        {
            // We do not check for null for performance reasons.
            //Ensure.NotNull(source, "source");

            this.source = source;
        }

        [HttpGet]
        [Route("streams/{id1}/{v1}/{id2}/{v2}/{id3}/{v3}/{id4}/{v4}/{id5}/{v5}")]
        public IHttpActionResult Streams(string id1, int v1, string id2, int v2, string id3, int v3, string id4, int v4, string id5, int v5)
        {
            var request = new PollRequest(id1, v1, id2, v2, id3, v3, id4, v4, id5, v5);
            var response = this.source.PollEvents(request);

            return this.Ok(response);
        }
    }
}
