using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollEventsResponse
    {
        public PollEventsResponse(bool pollingWasSuccessful, List<PolledEventData> events)
        {
            this.PollingWasSuccessful = pollingWasSuccessful;
            this.Events = events;
        }

        public bool PollingWasSuccessful { get; private set; }

        public List<PolledEventData> Events { get; private set; }
    }
}
