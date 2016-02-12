using EventCentric.Log;
using Microsoft.AspNet.SignalR;

namespace EventCentric.Factory
{
    public class LogHub : Hub
    {
        private readonly ILogger logger = SignalRLogger.ResolvedSignalRLogger;

        // This is just to show messages when a client is connected!
        public void SendMessage(string message)
        {
            this.Clients.All.newMessage(message);

            if (this.logger != null)
                this.logger.Trace(message);
        }
    }
}
