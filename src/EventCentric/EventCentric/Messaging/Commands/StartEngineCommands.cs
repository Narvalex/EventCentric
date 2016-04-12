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
}
