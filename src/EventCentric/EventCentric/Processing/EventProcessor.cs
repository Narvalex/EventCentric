using EventCentric.Messaging;

namespace EventCentric.Processing
{
    public class EventProcessor : Worker
    {
        public EventProcessor(IBus bus)
            : base(bus)
        {

        }
    }
}
