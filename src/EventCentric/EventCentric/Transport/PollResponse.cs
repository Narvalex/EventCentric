using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        public PollResponse(bool newEventsWereFound, string streamType, List<NewRawEvent> newEvents)
        {
            this.NewEventsWereFound = newEventsWereFound;
            this.StreamType = streamType;
            this.NewEvents = newEvents;
        }

        public bool NewEventsWereFound { get; private set; }
        public string StreamType { get; private set; }
        public List<NewRawEvent> NewEvents { get; private set; }
    }
}
