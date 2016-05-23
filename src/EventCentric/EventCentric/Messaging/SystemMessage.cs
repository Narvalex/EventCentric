using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace EventCentric.Messaging
{
    public abstract class SystemMessage
    {
        protected static int NextMessageId = -1;
        private static readonly int TypeId = Interlocked.Increment(ref NextMessageId);
        public virtual int MessageTypeId => TypeId;
    }

    public static class SystemMessageIdGuardian
    {
        public static readonly Dictionary<Type, int> MessageTypeIdByType;
        public static int MaxMessageTypeId;

        static SystemMessageIdGuardian()
        {
            MessageTypeIdByType = new Dictionary<Type, int>();
            var rootMessageType = typeof(SystemMessage);

            int messageTypeCount = 0;
            AppDomain.CurrentDomain
            .GetAssemblies()
            .ForEach(a =>
                a.GetTypes()
                .Where(t => rootMessageType.IsAssignableFrom(t))
                .ForEach(messageType =>
                {
                    messageTypeCount += 1;

                    var messageTypeId = GetMessageTypeId(messageType);
                    MessageTypeIdByType.Add(messageType, messageTypeId);

                    MaxMessageTypeId = Math.Max(messageTypeId, MaxMessageTypeId);

                }));

            if (messageTypeCount - 1 != MaxMessageTypeId)
                throw new Exception("Incorrect Message Type IDs setup.");
        }

        private static int GetMessageTypeId(Type messageType)
        {
            int typeId;
            if (MessageTypeIdByType.TryGetValue(messageType, out typeId))
                return typeId;

            var messageTypeField = messageType.GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                                              .FirstOrDefault(x => x.Name == "TypeId");

            if (messageTypeField == null)
                throw new Exception($"Message {messageType.Name} does not have TypeId filed!");

            return (int)messageTypeField.GetValue(null);
        }
    }
}
