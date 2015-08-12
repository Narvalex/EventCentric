using System;
using System.Threading.Tasks;

namespace EventCentric.Transport
{
    public interface IHttpClient : IDisposable
    {
        Task<string> GetStringAsync(string requestUri);
    }
}
