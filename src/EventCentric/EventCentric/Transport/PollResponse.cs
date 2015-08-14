using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        public PollResponse(List<PolledEventData> events)
        {
            this.Events = events;
        }

        public List<PolledEventData> Events { get; private set; }
    }
}
