using EventCentric.Utils;

namespace EventCentric.Messaging
{
    public interface IBus
    {
        // It is not for high contention 
        void Publish(SystemMessage message);
    }

    public interface ISystemHandler<T> where T : SystemMessage
    {
        void Handle(T message);
    }

    public interface IBusRegistry
    {
        void Register<T>(ISystemHandler<T> handler) where T : SystemMessage;
    }

    internal interface IMessageHandler
    {
        void TryHandle(SystemMessage message);
    }

    internal class MessageHandler<T> : IMessageHandler where T : SystemMessage
    {
        private readonly ISystemHandler<T> handler;

        public MessageHandler(ISystemHandler<T> handler)
        {
            Ensure.NotNull(handler, nameof(handler));

            this.handler = handler;
        }

        public void TryHandle(SystemMessage message)
        {
            this.handler.Handle(message as T);
        }
    }
}
