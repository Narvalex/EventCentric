using System;
using System.Net.Http;

namespace EventCentric.Transport
{
    public class HttpPoller : IHttpPoller
    {
        public PollEventsResponse PollEvents(string url)
        {
            using (var client = this.CreateHttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<PollEventsResponse>().Result;
                    else
                        return new PollEventsResponse(false, null);
                }
                catch
                {
                    return new PollEventsResponse(false, null);
                }

            }
        }

        public PollStreamsResponse PollStreams(string url)
        {
            using (var client = this.CreateHttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<PollStreamsResponse>().Result;
                    else
                        return new PollStreamsResponse(false, null, null);
                }
                catch
                {
                    return new PollStreamsResponse(false, null, null);
                }

            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 30);
            return client;
        }
    }
}
