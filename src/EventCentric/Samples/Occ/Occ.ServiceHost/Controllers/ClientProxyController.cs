using EventCentric.Authorization;
using EventCentric.Transport;
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
        public IHttpActionResult Upload([FromBody]PollResponse response)
        {
            var proxy = UnityConfig.GetClientProxy(response.StreamType);
            return this.Ok(proxy.UpdateServer(response));
        }
    }
}
