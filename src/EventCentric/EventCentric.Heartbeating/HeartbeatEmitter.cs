using EventCentric.Log;
using EventCentric.Polling;
using System.Net;

namespace EventCentric.Heartbeating
{
    public static class HeartbeatEmitter
    {
        public static string GetHeartbeat(INode node, ILogger log, IMonitoredSubscriber subscriber)
        {
            // More info: https://msdn.microsoft.com/en-us/library/system.net.ipaddress.loopback(v=vs.110).aspx
            var ipAddressString = IPAddress.Loopback.ToString();

            var responseText = $"Heartbeat of {node.Name} from {ipAddressString} | Status {node.State.ToString()}";
            foreach (var subscription in subscriber.GetSubscriptionsMetrics())
                responseText += $" | {subscription.ProducerName} subscription is in version {subscription.ConsumerVersion}/{subscription.ProducerVersion} {subscription.UpToDatePercentage.ToString("N2")}%";
            return responseText;
        }
    }
}
