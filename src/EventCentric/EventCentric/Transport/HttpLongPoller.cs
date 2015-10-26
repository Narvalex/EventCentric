using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

// More info: http://stackoverflow.com/questions/17700089/whats-the-best-way-to-target-multiple-versions-of-the-net-framework
// and: https://msdn.microsoft.com/en-us/library/aa691098(v=vs.71).aspx


namespace EventCentric.Transport
{
    public class HttpLongPoller : Worker, IHttpLongPoller
    {
        private readonly TimeSpan timeout;
        private readonly ILogger log;
        private readonly string pollerName;

        public HttpLongPoller(IBus bus, ILogger log, TimeSpan timeout, string pollerName)
            : base(bus)
        {
            Ensure.NotNullEmtpyOrWhiteSpace(pollerName, "pollerName");
            Ensure.NotNull(log, "log");
            if (timeout.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("timeout", "The timeout value must be greater than one second.");

            this.timeout = timeout;
            this.log = log;
            this.pollerName = pollerName;
        }

        public void PollSubscription(string streamType, string url, string token, long fromVersion)
        {
            // when poll arives, publish in bus.

            using (var httpClient = this.CreateHttpClient(token))
            {
                try
                {
                    var result = httpClient.GetAsync($"{url}/{fromVersion}/{this.pollerName}").Result;
                    if (!result.IsSuccessStatusCode)
                        throw new InvalidOperationException(string.Format("The status code was: {0}", result.StatusCode.ToString()));

                    var response = result.Content.ReadAsAsync<PollResponse>().Result;

                    this.bus.Publish(new PollResponseWasReceived(response));
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, "Error while polling {0}, from {1}, from version {2}", streamType, url, fromVersion);
                    // To have a break;
                    Thread.Sleep(10000);
                    this.bus.Publish(new PollResponseWasReceived(new PollResponse(true, false, streamType, null, 0, 0)));
                }
            }
        }


        /// <summary>
        /// More info on credentials: http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
        /// </summary>
        /// <returns></returns>
        private HttpClient CreateHttpClient(string token)
        {
            var client = new HttpClient();
            client.Timeout = this.timeout;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
