using System;
using System.Net.Http;

namespace EventCentric.Transport
{
    public class HttpPoller : IHttpPoller
    {
        public PollEventsResponse PollEvents(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                return response.Content.ReadAsAsync<PollEventsResponse>().Result;
            }
        }

        public PollStreamsResponse PollStreams(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync(url).Result;
                    return response.Content.ReadAsAsync<PollStreamsResponse>().Result;
                }
                catch (Exception)
                {
                    return new PollStreamsResponse(false, null, null);
                }

            }
        }
    }
}
