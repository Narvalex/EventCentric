using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollEventsResponse
    {
        public PollEventsResponse(List<PolledEventData> events)
        {
            this.Events = events;
        }

        public List<PolledEventData> Events { get; private set; }
    }
}
