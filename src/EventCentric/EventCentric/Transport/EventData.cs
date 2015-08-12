using System;

namespace EventCentric.Transport
{
    public class EventData
    {
        public EventData(bool isNewEvent, string payload)
        {
            this.IsNewEvent = isNewEvent;
            this.Payload = payload;
        }

        public bool IsNewEvent { get; private set; }

        public Guid StreamId { get; set; }

        public string Payload { get; private set; }
    }
}
