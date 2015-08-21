using EventCentric.Utils;
using System;
using System.Collections.Generic;

namespace EventCentric.Transport
{
    public class PollEventsRequest
    {
        public PollEventsRequest(string id1, int v1, string id2, int v2, string id3, int v3, string id4, int v4, string id5, int v5)
        {
            this.StreamVersionsFromSubscriber = new List<KeyValuePair<Guid, int>>(5);

            if (!id1.IsEncodedEmptyString())
                this.StreamVersionsFromSubscriber.Add(new KeyValuePair<Guid, int>(new Guid(id1), v1));

            if (!id2.IsEncodedEmptyString())
                this.StreamVersionsFromSubscriber.Add(new KeyValuePair<Guid, int>(new Guid(id2), v2));

            if (!id3.IsEncodedEmptyString())
                this.StreamVersionsFromSubscriber.Add(new KeyValuePair<Guid, int>(new Guid(id3), v3));

            if (!id4.IsEncodedEmptyString())
                this.StreamVersionsFromSubscriber.Add(new KeyValuePair<Guid, int>(new Guid(id4), v4));

            if (!id5.IsEncodedEmptyString())
                this.StreamVersionsFromSubscriber.Add(new KeyValuePair<Guid, int>(new Guid(id5), v5));

        }
        public List<KeyValuePair<Guid, int>> StreamVersionsFromSubscriber { get; private set; }
    }
}
