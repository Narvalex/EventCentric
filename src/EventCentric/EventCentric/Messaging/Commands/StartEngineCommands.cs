namespace EventCentric.Messaging.Commands
{
    public struct StartEventPublisher : IMessage { }

    public struct StartEventProcessor : IMessage { }

    public struct StartEventPoller : IMessage
    {
        public StartEventPoller(string microserviceName)
        {
            this.MicroserviceName = microserviceName;
        }

        public string MicroserviceName { get; }
    }
}
