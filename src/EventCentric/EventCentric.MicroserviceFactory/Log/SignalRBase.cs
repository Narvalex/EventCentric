using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;

namespace EventCentric.Factory
{
    /// <summary>
    /// A base abstract class which allows to 
    /// integrate a SignalR hub into any class – in our case it will be 
    /// the ITraceWriter but that same base can be used to integrate a 
    /// hub into i.e. our controllers.
    /// Source: http://www.strathweb.com/2012/11/realtime-asp-net-web-api-tracing-with-signalr/
    /// </summary>
    public abstract class SignalRBase<T> where T : IHub
    {
        private Lazy<IHubContext> hub = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<T>());

        protected IHubContext Hub
        {
            get { return hub.Value; }
        }
    }
}
