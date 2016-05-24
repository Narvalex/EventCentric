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
    public class LongPoller : Worker, ILongPoller
    {
        private readonly TimeSpan timeout;
        private readonly ILogger log;
        private readonly string pollerName;
        private readonly IInMemoryEventPublisher inMemoryPublisher;

        public LongPoller(IBus bus, ILogger log, TimeSpan timeout, string pollerName, IInMemoryEventPublisher inMemoryPublisher)
            : base(bus)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(pollerName, "pollerName");
            Ensure.NotNull(log, "log");
            if (timeout.TotalSeconds <= 1)
                throw new ArgumentOutOfRangeException("timeout", "The timeout value must be greater than one second.");

            this.inMemoryPublisher = inMemoryPublisher; //if it is null, then it means that the poller is absolutely remote.
            this.timeout = timeout;
            this.log = log;
            this.pollerName = pollerName;

            // More info:http://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
            this.pollerName = new string(pollerName.Where(x => x != '.').ToArray());
        }

        public void PollSubscription(string streamType, string url, string token, long fromVersion)
        {
            if (url == Constants.InMemorySusbscriptionUrl)
                this.PollInMemory(streamType, fromVersion);
            else
                this.PollFromHttp(streamType, url, token, fromVersion);
        }

        private void PollInMemory(string streamType, long fromVersion)
        {
            var response = this.inMemoryPublisher.PollEvents(streamType, fromVersion, this.pollerName);
            this.PublishPollResponse(response);
        }

        private void PollFromHttp(string streamType, string url, string token, long fromVersion)
        {
            using (var httpClient = this.CreateHttpClient(token))
            {
                var dynamicUrl = $"{url}/{fromVersion}/{this.pollerName}";
                try
                {
                    var result = httpClient.GetAsync(dynamicUrl).Result;
                    if (!result.IsSuccessStatusCode)
                        throw new InvalidOperationException(string.Format("The status code was: {0}", result.StatusCode.ToString()));

                    var response = result.Content.ReadAsAsync<PollResponse>().Result;

                    this.PublishPollResponse(response);
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, $"Error while polling {streamType} located on {dynamicUrl}");
                    this.log.Trace($"An error has been detected while polling {streamType} located on {dynamicUrl} but a retry will be performed every 10 seconds");

                    // To have a break;
                    Thread.Sleep(10000);
                    this.bus.Publish(new PollResponseWasReceived(PollResponse.CreateErrorResponse(streamType)));
                }
            }
        }

        private void PublishPollResponse(PollResponse response)
        {
            this.bus.Publish(new PollResponseWasReceived(response));
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
