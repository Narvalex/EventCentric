using System;

namespace EventCentric.Transport
{
    public class EventData
    {
        public EventData(bool isNewEvent, string streamType, Guid streamId, string payload)
        {
            this.IsNewEvent = isNewEvent;
            this.StreamType = streamType;
            this.StreamId = streamId;
            this.Payload = payload;
        }

        public bool IsNewEvent { get; private set; }

        public string StreamType { get; private set; }

        public Guid StreamId { get; private set; }

        public string Payload { get; private set; }
    }
}
