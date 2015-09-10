using EventCentric.Publishing;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyTrade.EmpresasQueue.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EventSourceController : ApiController
    {
        private readonly IEventSource source;

        public EventSourceController(IEventSource source)
        {
            this.source = source;
        }

        [HttpGet]
        [Route("eventsource/events/{eventBufferVersion}")]
        public IHttpActionResult Events(int eventBufferVersion)
        {
            return this.Ok(this.source.PollEvents(eventBufferVersion));
        }
    }
}
