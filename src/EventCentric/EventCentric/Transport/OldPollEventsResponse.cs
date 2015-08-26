using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class OldPollEventsResponse
    {
        public OldPollEventsResponse(bool pollingWasSuccessful, List<OldPolledEventData> events)
        {
            this.PollingWasSuccessful = pollingWasSuccessful;
            this.Events = events;
        }

        public bool PollingWasSuccessful { get; private set; }

        public List<OldPolledEventData> Events { get; private set; }
    }
}
