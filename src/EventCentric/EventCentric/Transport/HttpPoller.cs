using System.Net.Http;

namespace EventCentric.Transport
{
    public class HttpPoller : IHttpPoller
    {
        public PollResponse Poll(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;
                return response.Content.ReadAsAsync<PollResponse>().Result;
            }
        }
    }
}
