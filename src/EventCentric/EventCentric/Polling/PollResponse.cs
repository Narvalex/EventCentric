using EventCentric.EventSourcing;
using EventCentric.Polling;
using System.Collections.Generic;

namespace EventCentric.Transport
{

    public class PollResponse
    {
        public static PollResponse CreateErrorResponse(string streamType)
        {
            return CreateBaseResponse(true, false, streamType, 0, 0);
        }

        private static PollResponse CreateBaseResponse(bool errorDetected, bool newEventsWereFound, string streamType, long consumerVersion, long producerVersion)
        {
            var response = new PollResponse();
            response.NewEventsWereFound = newEventsWereFound;
            response.StreamType = streamType;
            response.ErrorDetected = errorDetected;

            // Metrics 
            response.ConsumerVersion = consumerVersion;
            response.ProducerVersion = producerVersion;
            return response;
        }

        public static PollResponse CreateSerializedResponse(bool errorDetected, bool newEventsWereFound, string streamType, IEnumerable<NewRawEvent> newRawEvents, long consumerVersion, long producerVersion)
        {
            var r = CreateBaseResponse(errorDetected, newEventsWereFound, streamType, consumerVersion, producerVersion);

            r.NewRawEvents = newRawEvents;
            r.IsSerialized = true;
            return r;
        }

        public static PollResponse CreateInMemoryResponse(bool errorDetected, bool newEventsWereFound, string streamType, IEnumerable<IEvent> events, long consumerVersion, long producerVersion)
        {
            var r = CreateBaseResponse(errorDetected, newEventsWereFound, streamType, consumerVersion, producerVersion);

            r.Events = events;
            r.IsSerialized = false;

            return r;
        }

        public PollResponse()
        { }

        public bool NewEventsWereFound { get; set; }
        public bool ErrorDetected { get; set; }
        public bool IsSerialized { get; set; }

        public string StreamType { get; set; }
        public IEnumerable<NewRawEvent> NewRawEvents { get; set; }
        public IEnumerable<IEvent> Events { get; set; }

        // Metrics
        public long ConsumerVersion { get; set; }
        public long ProducerVersion { get; set; }
    }
}
