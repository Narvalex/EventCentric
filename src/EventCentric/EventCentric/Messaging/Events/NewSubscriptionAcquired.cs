using System;

namespace EventCentric.Messaging.Events
{
    public class NewSubscriptionAcquired : IMessage
    {
        public NewSubscriptionAcquired(string streamType, Guid streamId)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
        }

        public string StreamType { get; private set; }
        public Guid StreamId { get; private set; }
    }
}
