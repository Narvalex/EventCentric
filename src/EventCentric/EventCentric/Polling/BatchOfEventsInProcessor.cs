using EventCentric.Transport;
using System.Collections.Generic;

namespace EventCentric.Polling
{
    public class BatchOfEventsInProcessor
    {
        public BatchOfEventsInProcessor(int eventsInProcessorVersion, List<NewEvent> events)
        {
            this.EventsInProcessorVersion = eventsInProcessorVersion;
            this.Events = events;
        }

        // The lowest version in the batch
        public int EventsInProcessorVersion { get; private set; }

        public List<NewEvent> Events { get; private set; }
    }
}
