using EventCentric.Authorization;
using EventCentric.Publishing.Dto;
using Occ.ServiceHost.App_Start;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Occ.ServiceHost.Controllers
{
    [RoutePrefix("client-proxy")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [AuthorizeEventSourcing]
    public class ClientProxyController : ApiController
    {
        [HttpPost]
        [Route("upload")]
        public IHttpActionResult Upload([FromBody]ClientData data)
        {
            var proxy = UnityConfig.GetClientProxy(data.PollResponse.StreamType);
            return this.Ok(proxy.UpdateServer(data));
        }
    }
}
