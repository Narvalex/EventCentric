using System.Net;
using System.Web.Mvc;

namespace EventCentric.Heartbeating
{
    public class HeartbeatController : Controller
    {
        // GET: Heartbeat
        public ActionResult Index()
        {
            // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
            var ipAddressString = IPAddress.Loopback.ToString();
            return this.Content(string.Format("A heartbeat from {0}", ipAddressString));
        }
    }
}
