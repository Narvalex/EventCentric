using EventCentric.Messaging;

namespace EventCentric.Publishing
{
    public class EventPublisher : Worker
    {
        public EventPublisher(IBus bus)
            : base(bus)
        {

        }
    }
}
