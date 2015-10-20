using EventCentric.Authorization;
using EventCentric.Publishing;
using System.Web.Http;
using System.Web.Http.Cors;

namespace InformesDeServicio.Publicadores.Server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [AuthorizationRequired]
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
