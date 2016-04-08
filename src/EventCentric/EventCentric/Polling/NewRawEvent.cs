using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class NewRawEvent
    {
        public NewRawEvent(long eventCollectionVersion, string payload)
        {
            this.EventCollectionVersion = eventCollectionVersion;
            this.Payload = payload;
        }

        public long EventCollectionVersion { get; }
        public string Payload { get; }
    }
}
