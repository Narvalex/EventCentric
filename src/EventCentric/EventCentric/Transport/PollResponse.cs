using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        public PollResponse(bool errorDetected, bool newEventsWereFound, string streamType, List<NewRawEvent> newEvents, int consumerVersion, int producerVersion)
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
        public int ConsumerVersion { get; private set; }
        public int ProducerVersion { get; private set; }
    }
}
