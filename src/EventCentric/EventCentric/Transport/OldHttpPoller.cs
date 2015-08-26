using System;
using System.Net.Http;

namespace EventCentric.Transport
{
    public class OldHttpPoller : IOldHttpPoller
    {
        public OldPollEventsResponse PollEvents(string url)
        {
            using (var client = this.CreateHttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<OldPollEventsResponse>().Result;
                    else
                        return new OldPollEventsResponse(false, null);
                }
                catch
                {
                    return new OldPollEventsResponse(false, null);
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
