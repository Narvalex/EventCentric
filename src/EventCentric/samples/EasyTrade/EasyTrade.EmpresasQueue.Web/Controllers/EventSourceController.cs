using EventCentric.Authorization;
using EventCentric.Publishing;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyTrade.EmpresasQueue.Web.Controllers
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
        [Route("eventsource/events/{eventBufferVersion}/{consumerName}")]
        public IHttpActionResult Events(int eventBufferVersion, string consumerName)
        {
            var response = this.source.PollEvents(eventBufferVersion, consumerName);
            return this.Ok(response);
        }
    }
}
