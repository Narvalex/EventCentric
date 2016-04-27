using EventCentric.Utils;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Heartbeating
{
    /// <summary>
    /// A heartbeat listener. Since a tipical event centric node is hosted in IIS there is always the risk that a 
    /// subscriber falls asleep. This is the responsability of the Heartbeat Listener. Keep the subscriber node 
    /// always alive.
    /// </summary>
    public class InMemoryHeartbeatListener
    {
        private readonly TimeSpan timeout;
        private readonly TimeSpan interval;
        private Tuple<string, string>[] subscribersNamesAndUrls;
        private readonly string nodeName;

        public InMemoryHeartbeatListener(string nodeName, TimeSpan timeout, TimeSpan interval, params Tuple<string, string>[] nodesToListenTo)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(nodeName, nameof(nodeName));
            if (timeout.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("timeout", "The timeout value must be greater than one second.");
            if (interval.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("interval", "The interval value must be greater than one second.");

            this.timeout = timeout;
            this.interval = interval;
            this.nodeName = nodeName;
            this.subscribersNamesAndUrls = nodesToListenTo;

            this.RequestHeartbeats();
        }

        private void RequestHeartbeats()
        {
            if (this.subscribersNamesAndUrls == null)
            {
                return;
            }

            foreach (var sub in this.subscribersNamesAndUrls)
                Task.Factory.StartNewLongRunning(() =>
                {
                    this.RequestHeartbeat(sub.Item1, sub.Item2);
                });
        }

        private void RequestHeartbeat(string name, string url)
        {
            while (true)
            {
                try
                {
                    using (var client = this.CreateHttpClient())
                    {
                        var response = client.GetAsync($"{url}/{this.nodeName}").Result;
                        if (!response.IsSuccessStatusCode)
                            throw new InvalidOperationException(string.Format("Heartbeat request received an status code of: {0}", response.StatusCode.ToString()));
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    Thread.Sleep(this.interval);
                }
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = this.timeout;
            return client;
        }
    }
}
