namespace EventCentric.Messaging
{
    public interface IMessage { }

    public interface ISystemBus
    {
        // It is not for hight contention 
        void Publish(IMessage message);
    }

    public interface ISystemHandler { }

    public interface IMessageHandler<T> : ISystemHandler where T : IMessage
    {
        void Handle(T message);
    }

    public interface IBusRegistry
    {
        void Register(ISystemHandler worker);
    }
}
