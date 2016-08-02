using EventCentric.Log;
using EventCentric.Polling;
using EventCentric.Utils;
using System.Net;

namespace EventCentric.Heartbeating
{
    /// <summary>
    /// A heartbeat emitter is attached to a poller (a subscriber). This belongs to a subscriber that 
    /// can fall asleep. The Listener is always asking for a heartbeat on a regular interval.
    /// </summary>
    public class HeartbeatEmitter
    {
        //private readonly IMicroservice node;
        //private readonly ILogger log;
        //private readonly IMonitoredSubscriber subscriber;

        private readonly string nodeName;

        public HeartbeatEmitter(string nodeName)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(nodeName, nameof(nodeName));

            this.nodeName = nodeName;
        }

        //public HeartbeatEmitter(IMicroservice node, ILogger log, IMonitoredSubscriber subscriber)
        //{
        //    Ensure.NotNull(node, nameof(node));
        //    Ensure.NotNull(log, nameof(log));
        //    Ensure.NotNull(subscriber, nameof(subscriber));

        //    this.node = node;
        //    this.log = log;
        //    this.subscriber = subscriber;
        //}

        //public string GetHeartbeat(string heartbeatListener)
        //{
        //    // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
        //    var ipAddressString = IPAddress.Loopback.ToString();

        //    var responseText = $"Heartbeat of {node.Name} from {ipAddressString} | Status {node.Status.ToString()}";
        //    foreach (var subscription in subscriber.GetSubscriptionsMetrics())
        //        responseText += $" | {subscription.ProducerName} subscription is in version {subscription.ConsumerVersion}/{subscription.ProducerVersion} {subscription.ConsistencyPercentage.ToString("N2")}%";

        //    log.Trace($"{heartbeatListener} requested a heartbeat.");

        //    return responseText;
        //}

        public string GetHeartbeat(string heartbeatListener)
        {
            // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
            var ipAddressString = IPAddress.Loopback.ToString();

            var responseText = $"Heartbeat of {this.nodeName} from {ipAddressString}";

            return responseText;
        }
    }
}
