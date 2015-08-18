using System.Net.Http;

namespace EventCentric.Transport
{
    public class SynchronousHttpClient : HttpClient, IHttpClient
    {
        public new void Dispose()
        {
            base.Dispose();
        }

        public string GetString(string requestUri)
        {
            return base.GetStringAsync(requestUri).Result;
        }
    }
}
