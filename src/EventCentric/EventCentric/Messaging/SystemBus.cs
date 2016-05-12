using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Messaging
{
    public class SystemBus : ISystemBus, IBusRegistry
    {
        private Dictionary<Type, List<ISystemHandler>> handlersByMessageType = new Dictionary<Type, List<ISystemHandler>>();

        public void Publish(IMessage message)
        {
            List<ISystemHandler> handlers;
            if (this.handlersByMessageType.TryGetValue(message.GetType(), out handlers))
                handlers.ForEach(handler => ((dynamic)handler).Handle((dynamic)message));
            else
                throw new InvalidOperationException($"There are any handler registered for system message of type {message.GetType().FullName}");
        }

        public void Register(ISystemHandler worker)
        {
            var genericHandler = typeof(IMessageHandler<>);
            worker
            .GetType()
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
            .Select(i => i.GetGenericArguments()[0])
            .ForEach(messageType =>
            {
                List<ISystemHandler> handlers;
                if (!this.handlersByMessageType.TryGetValue(messageType, out handlers))
                {
                    handlers = new List<ISystemHandler>();
                    this.handlersByMessageType[messageType] = handlers;
                }
                handlers.Add(worker);
            });
        }
    }
}
