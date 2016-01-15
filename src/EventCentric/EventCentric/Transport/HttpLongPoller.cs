using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

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
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(pollerName, "pollerName");
            Ensure.NotNull(log, "log");
            if (timeout.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("timeout", "The timeout value must be greater than one second.");

            this.timeout = timeout;
            this.log = log;

            // More info:http://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
            this.pollerName = new string(pollerName.Where(x => x != '.').ToArray());
        }

        public void PollSubscription(string streamType, string url, string token, long fromVersion)
        {
            // when poll arives, publish in bus.
            using (var httpClient = this.CreateHttpClient(token))
            {
                var dynamicUrl = $"{url}/{fromVersion}/{this.pollerName}";
                try
                {
                    var result = httpClient.GetAsync(dynamicUrl).Result;
                    if (!result.IsSuccessStatusCode)
                        throw new InvalidOperationException(string.Format("The status code was: {0}", result.StatusCode.ToString()));

                    var response = result.Content.ReadAsAsync<PollResponse>().Result;

                    this.bus.Publish(new PollResponseWasReceived(response));
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, $"Error while polling {streamType} located on {dynamicUrl}");
                    this.log.Trace($"An error has been detected while polling {streamType} located on {dynamicUrl} but a retry will be performed every 10 seconds");

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
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
