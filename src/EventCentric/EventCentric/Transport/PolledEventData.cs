using System;

namespace EventCentric.Transport
{
    public class PolledEventData
    {
        public PolledEventData(string streamType, Guid streamId, bool isNewEvent, string payload)
        {
            this.StreamType = streamType;
            this.StreamId = streamId;
            this.IsNewEvent = isNewEvent;
            this.Payload = payload;
        }

        public bool IsNewEvent { get; private set; }

        public string StreamType { get; private set; }

        public Guid StreamId { get; private set; }

        public string Payload { get; private set; }
    }
}
