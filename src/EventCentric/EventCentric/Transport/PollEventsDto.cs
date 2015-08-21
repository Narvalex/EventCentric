using System;
using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollEventsDto
    {
        public PollEventsDto(string streamType, string url, Guid streamId, int version)
        {
            this.StreamType = streamType;
            this.Url = url;
            this.ProcessedStreams = new List<KeyValuePair<Guid, int>>(5);
            this.Add(streamId, version);
        }

        public void Add(Guid streamId, int version)
        {
            this.ProcessedStreams.Add(new KeyValuePair<Guid, int>(streamId, version));
        }

        public string StreamType { get; private set; }
        public string Url { get; private set; }
        public List<KeyValuePair<Guid, int>> ProcessedStreams { get; private set; }
    }
}
