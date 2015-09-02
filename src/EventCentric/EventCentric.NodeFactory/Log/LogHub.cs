using EventCentric.Log;
using Microsoft.AspNet.SignalR;

namespace EventCentric.NodeFactory.Log
{
    public class LogHub : Hub
    {
        private readonly ILogger logger = Logger.ResolvedLogger;

        // This is just to show messages when a client is connected!
        public void SendMessage(string message)
        {
            this.Clients.All.newMessage(message);

            if (this.logger != null)
                this.logger.Trace(message);
        }
    }
}
