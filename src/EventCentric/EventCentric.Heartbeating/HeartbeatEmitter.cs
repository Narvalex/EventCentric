using EventCentric.Log;
using EventCentric.Polling;
using EventCentric.Utils;
using System.Net;

namespace EventCentric.Heartbeating
{
    /// <summary>
    /// A heartbeat emitter is attached to a poller (a subscriber)
    /// </summary>
    public class HeartbeatEmitter
    {
        private readonly IMicroservice node;
        private readonly ILogger log;
        private readonly IMonitoredSubscriber subscriber;

        public HeartbeatEmitter(IMicroservice node, ILogger log, IMonitoredSubscriber subscriber)
        {
            Ensure.NotNull(node, nameof(node));
            Ensure.NotNull(log, nameof(log));
            Ensure.NotNull(subscriber, nameof(subscriber));

            this.node = node;
            this.log = log;
            this.subscriber = subscriber;
        }

        public string GetHeartbeat(string heartbeatListener)
        {
            // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
            var ipAddressString = IPAddress.Loopback.ToString();

            var responseText = $"Heartbeat of {node.Name} from {ipAddressString} | Status {node.State.ToString()}";
            foreach (var subscription in subscriber.GetSubscriptionsMetrics())
                responseText += $" | {subscription.ProducerName} subscription is in version {subscription.ConsumerVersion}/{subscription.ProducerVersion} {subscription.UpToDatePercentage.ToString("N2")}%";

            log.Trace($"{heartbeatListener} requested a heartbeat.");

            return responseText;
        }
    }
}
