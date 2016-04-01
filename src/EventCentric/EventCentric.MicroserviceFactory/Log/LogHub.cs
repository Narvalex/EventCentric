using EventCentric.Log;
using Microsoft.AspNet.SignalR;

namespace EventCentric.Factory
{
    public class LogHub : Hub
    {
        private readonly ILogger logger = SignalRLogger.GetResolvedSignalRLogger();

        // This is just to show messages when a client is connected!
        public void SendMessage(string message)
        {
            this.Clients.All.newMessage(message);

            if (this.logger != null)
            {
                this.logger.Log($"{message}. " + (SignalRLogger._Verbose ? "Verbose logging is ENABLED" : "Verbose logging is disabled"));
            }
        }
    }
}
