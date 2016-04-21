namespace EventCentric.Messaging.Commands
{
    public struct StopEventPoller : IMessage
    {
        public StopEventPoller(bool stopSilently = false)
        {
            this.StopSilently = stopSilently;
        }

        public bool StopSilently { get; }
    }

    public struct StopEventProcessor : IMessage { }

    public struct StopEventPublisher : IMessage { }
}
