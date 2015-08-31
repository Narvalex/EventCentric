﻿#define NET4

using EventCentric.Messaging;
using EventCentric.Messaging.Events;
using System;
using System.Net.Http;

// More info: http://stackoverflow.com/questions/17700089/whats-the-best-way-to-target-multiple-versions-of-the-net-framework
// and: https://msdn.microsoft.com/en-us/library/aa691098(v=vs.71).aspx


namespace EventCentric.Transport
{
    public class HttpPollster : Worker, IHttpPollster
    {
        private const int timeoutSeconds = 60;

        public HttpPollster(IBus bus)
            : base(bus)
        { }

        public void PollSubscription(string streamType, string url, int fromVersion)
        {
            // when poll arives, publish in bus.

            using (var client = this.CreateHttpClient())
            {
                try
                {
                    var getResult = client.GetAsync(string.Format("{0}/{1}", url, fromVersion)).Result;
                    if (!getResult.IsSuccessStatusCode)
                        throw new InvalidOperationException(string.Format("The status code was: {0}", getResult.StatusCode.ToString()));

#if NET4
                    var response = getResult.Content.ReadAsAsync<PollResponse>().Result;
#endif
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