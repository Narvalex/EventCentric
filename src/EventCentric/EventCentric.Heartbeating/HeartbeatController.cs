using EventCentric.Log;
using EventCentric.Polling;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EventCentric.Heartbeating
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class HeartbeatController : ApiController
    {
        private readonly INode node;
        private readonly ILogger log;
        private readonly IMonitoredSubscriber subscriber;

        public HeartbeatController(INode node, ILogger log, IMonitoredSubscriber subscriber)
        {
            this.node = node;
            this.log = log;
            this.subscriber = subscriber;
        }

        // GET: Heartbeat
        [HttpGet]
        [Route("heartbeat")]
        public IHttpActionResult Get()
        {
            // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
            var ipAddressString = IPAddress.Loopback.ToString();

            var responseText = $"Heartbeat of {this.node.Name} from {ipAddressString} that is {node.State.ToString()}";
            foreach (var subscription in subscriber.GetSubscriptionsMetrics())
                responseText += $" | Subscription of {subscription.ProducerName} is in version {subscription.ConsumerVersion} of {subscription.ProducerVersion} [{subscription.UpToDatePercentage.ToString("N2")}%]";

            return this.Ok(responseText);
        }
    }
}
