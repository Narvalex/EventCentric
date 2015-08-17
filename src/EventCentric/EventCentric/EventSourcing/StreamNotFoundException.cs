using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace EventCentric.EventSourcing
{
    [Serializable]
    public class StreamNotFoundException : Exception
    {
        private readonly Guid entityId;
        private readonly string entityType;

        public StreamNotFoundException()
        { }

        public StreamNotFoundException(Guid entityId) : base(entityId.ToString())
        {
            this.entityId = entityId;
        }

        public StreamNotFoundException(Guid entityId, string entityType)
            : base(entityType + ": " + entityId.ToString())
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        public StreamNotFoundException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.entityId = entityId;
            this.entityType = entityType;
        }

        protected StreamNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.entityId = Guid.Parse(info.GetString("entityId"));
            this.entityType = info.GetString("entityType");
        }

        public Guid EntityId
        {
            get { return this.entityId; }
        }

        public string EntityType
        {
            get { return this.entityType; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("entityId", this.entityId.ToString());
            info.AddValue("entityType", this.entityType);
        }
    }
}
