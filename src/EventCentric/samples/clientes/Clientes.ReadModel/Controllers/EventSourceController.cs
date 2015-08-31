using System.Web.Http;

namespace EventCentric.Publishing
{
    public class EventSourceController : ApiController
    {
        private readonly IEventSource source;

        public EventSourceController(IEventSource source)
        {
            // We do not check null for performance reasons.
            //Ensure.NotNull(source, "source");

            this.source = source;
        }

        [HttpGet]
        [Route("events/{eventBufferVersion}")]
        public IHttpActionResult Events(int eventBufferVersion)
        {
            var response = this.source.PollEvents(eventBufferVersion);
            return this.Ok(response);
        }
    }
}
