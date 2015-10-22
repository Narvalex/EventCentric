using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        public PollResponse(bool errorDetected, bool newEventsWereFound, string streamType, List<NewRawEvent> newEvents, long consumerVersion, long producerVersion)
        {
            this.NewEventsWereFound = newEventsWereFound;
            this.StreamType = streamType;
            this.NewEvents = newEvents;
            this.ErrorDetected = errorDetected;

            // Metrics 
            this.ConsumerVersion = consumerVersion;
            this.ProducerVersion = producerVersion;
        }

        public bool NewEventsWereFound { get; private set; }
        public bool ErrorDetected { get; private set; }

        public string StreamType { get; private set; }
        public List<NewRawEvent> NewEvents { get; private set; }

        // Metrics
        public long ConsumerVersion { get; private set; }
        public long ProducerVersion { get; private set; }
    }
}
