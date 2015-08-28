using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System;
using System.Net.Http;

namespace EventCentric.Transport
{
    public class HttpPoller : Worker, IHttpPoller
    {
        private const int timeoutSeconds = 60;

        public HttpPoller(IBus bus)
            : base(bus)
        { }

        public void PollSubscription(string streamType, string url, int fromVersion)
        {
            // when poll arives, publish in bus.

            using (var client = this.CreateHttpClient())
            {
                try
                {
                    var getResult = client.GetAsync($"{url}/events/{fromVersion}").Result;
                    if (!getResult.IsSuccessStatusCode)
                        throw new InvalidOperationException($"The status code was: {getResult.StatusCode.ToString()}");

                    var response = getResult.Content.ReadAsAsync<PollResponse>().Result;
                    this.bus.Publish(new PollResponseWasReceived(response));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    this.bus.Publish(new PollResponseWasReceived(new PollResponse(false, streamType, null)));
                }
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, timeoutSeconds);
            return client;
        }
    }
}
