using System;

namespace EventCentric.Transport
{
    public interface IHttpClient : IDisposable
    {
        string GetString(string requestUri);
    }
}
