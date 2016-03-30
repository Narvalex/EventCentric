using System;
using System.Collections.Generic;

namespace EventCentric.Messaging.Events
{
    public class IncomingEventsHasBeenProcessed : IMessage
    {
        public IncomingEventsHasBeenProcessed(List<Tuple<string, long>> events)
        {
            this.Events = events;
        }

        public List<Tuple<string, long>> Events { get; set; }
    }
}