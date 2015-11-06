using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace EventCentric.EventSourcing
{
    [Serializable]
    public class StreamNotFoundException : Exception
    {
        private readonly Guid streamId;
        private readonly string streamType;

        public StreamNotFoundException()
        { }

        public StreamNotFoundException(Guid entityId) : base(entityId.ToString())
        {
            this.streamId = entityId;
        }

        public StreamNotFoundException(Guid entityId, string entityType)
            : base(entityType + ": " + entityId.ToString())
        {
            this.streamId = entityId;
            this.streamType = entityType;
        }

        public StreamNotFoundException(Guid entityId, string entityType, string message, Exception inner)
            : base(message, inner)
        {
            this.streamId = entityId;
            this.streamType = entityType;
        }

        protected StreamNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.streamId = Guid.Parse(info.GetString("streamId"));
            this.streamType = info.GetString("streamType");
        }

        public Guid StreamId
        {
            get { return this.streamId; }
        }

        public string StreamType
        {
            get { return this.streamType; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("streamId", this.streamId.ToString());
            info.AddValue("streamType", this.streamType);
        }
    }
}
