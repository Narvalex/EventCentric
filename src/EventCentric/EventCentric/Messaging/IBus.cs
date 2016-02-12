namespace EventCentric.Messaging
{
    public interface IMessage { }

    public interface IBus
    {
        // It is not hight contention 
        void Publish(IMessage message);
    }

    public interface IWorker { }

    public interface IMessageHandler<T> : IWorker where T : IMessage
    {
        void Handle(T message);
    }

    public interface IBusRegistry
    {
        void Register(IWorker worker);
    }
}
