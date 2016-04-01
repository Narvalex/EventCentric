namespace EventCentric.Messaging.Commands
{
    public class StartEventPublisher : IMessage { }

    public class StartEventProcessor : IMessage { }

    public class StartEventPoller : IMessage
    {
        public StartEventPoller(string microserviceName)
        {
            this.MicroserviceName = microserviceName;
        }

        public string MicroserviceName { get; }
    }

    /// <summary>
    /// This could be used to start any service that does not belong to an order of intialization.
    /// </summary>
    public class StartHeartbeatListener : IMessage { }
}
