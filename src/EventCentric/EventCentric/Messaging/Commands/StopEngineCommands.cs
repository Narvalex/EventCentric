namespace EventCentric.Messaging.Commands
{
    public class StopEventPollster : IMessage { }

    public class StopEventProcessor : IMessage { }

    public class StopEventPublisher : IMessage { }

    public class StopEventQueue : IMessage { }
}
