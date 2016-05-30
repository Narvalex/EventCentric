using EventCentric.Utils;
using System;
using System.Collections.Generic;

namespace EventCentric.Messaging
{
    /// <summary>
    ///  A synchronous bus. Sync is more faster!
    /// </summary>
    public class Bus : IBus, IBusRegistry
    {
        private readonly List<IMessageHandler>[] handlers;

        public Bus()
        {
            this.handlers = new List<IMessageHandler>[SystemMessageIdProvider.MaxMessageTypeId + 1];
            for (int i = 0; i < this.handlers.Length; i++)
                handlers[i] = new List<IMessageHandler>();
        }

        public void Publish(SystemMessage message)
        {
            var handlers = this.handlers[message.MessageTypeId];
            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                handler.TryHandle(message);
            }
        }

        public void Register<T>(ISystemHandler<T> handler) where T : SystemMessage
        {
            Ensure.NotNull(handler, nameof(handler));

            this.handlers[SystemMessageIdProvider.MessageTypeIdByType[typeof(T)]]
                .Add(new MessageHandler<T>(handler));
        }
    }
}
