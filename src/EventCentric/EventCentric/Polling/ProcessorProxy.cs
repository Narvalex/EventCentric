using EventCentric.Messaging;
using EventCentric.Utils;
using System;

namespace EventCentric.Polling
{
    public class ProcessorProxy
    {
        private readonly IBus bus;

        public ProcessorProxy(IBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

        public bool TryPushEvents()
        {
            throw new NotImplementedException();
        }
    }
}
