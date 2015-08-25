using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EventCentric.Transport
{
    public class HttpPoller : IHttpPoller
    {
        public PollEventsResponse PollEvents(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var timeout = client.Timeout;
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<PollEventsResponse>().Result;
                    else
                        return new PollEventsResponse(new List<PolledEventData> { new PolledEventData("", Guid.Empty, false, "") });
                }
                catch (Exception)
                {
                    return new PollEventsResponse(new List<PolledEventData> { new PolledEventData("", Guid.Empty, false, "") });
                }

            }
        }

        public PollStreamsResponse PollStreams(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var timeout = client.Timeout;
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsAsync<PollStreamsResponse>().Result;
                    else
                        return new PollStreamsResponse(false, null, null);
                }
                catch (Exception)
                {
                    return new PollStreamsResponse(false, null, null);
                }

            }
        }
    }
}
