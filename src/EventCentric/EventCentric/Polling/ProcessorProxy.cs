using EventCentric.Messaging;
using System;

namespace EventCentric.Polling
{
    public class ProcessorProxy : Worker
    {
        public ProcessorProxy(IBus bus)
            : base(bus)
        { }

        public bool TryPushEvents()
        {
            throw new NotImplementedException();
        }
    }
}
