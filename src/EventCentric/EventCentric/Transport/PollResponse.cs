using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        public PollResponse(List<EventData> events)
        {
            this.Events = events;
        }

        public List<EventData> Events { get; private set; }
    }
}
