namespace EventCentric.Messaging.Commands
{
    public struct StopEventPoller : IMessage { }

    public struct StopEventProcessor : IMessage { }

    public struct StopEventPublisher : IMessage { }
}
