using EventCentric.EventSourcing;
using EventCentric.Polling;
using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollResponse
    {
        private PollResponse(bool errorDetected, bool newEventsWereFound, string streamType, long consumerVersion, long producerVersion)
        {
            this.NewEventsWereFound = newEventsWereFound;
            this.StreamType = streamType;
            this.ErrorDetected = errorDetected;

            // Metrics 
            this.ConsumerVersion = consumerVersion;
            this.ProducerVersion = producerVersion;

        }

        // ON ERROR
        public PollResponse(string streamType)
            : this(true, false, streamType, 0, 0)
        { }

        public PollResponse(bool errorDetected, bool newEventsWereFound, string streamType, IEnumerable<NewRawEvent> newRawEvents, long consumerVersion, long producerVersion)
            : this(errorDetected, newEventsWereFound, streamType, consumerVersion, producerVersion)
        {
            this.NewRawEvents = newRawEvents;
            this.IsSerialized = true;
        }

        public PollResponse(bool errorDetected, bool newEventsWereFound, string streamType, IEnumerable<IEvent> events, long consumerVersion, long producerVersion)
            : this(errorDetected, newEventsWereFound, streamType, consumerVersion, producerVersion)
        {
            this.Events = events;
            this.IsSerialized = false;
        }

        public bool NewEventsWereFound { get; private set; }
        public bool ErrorDetected { get; private set; }
        public bool IsSerialized { get; }

        public string StreamType { get; private set; }
        public IEnumerable<NewRawEvent> NewRawEvents { get; private set; }
        public IEnumerable<IEvent> Events { get; private set; }

        // Metrics
        public long ConsumerVersion { get; private set; }
        public long ProducerVersion { get; private set; }
    }
}
