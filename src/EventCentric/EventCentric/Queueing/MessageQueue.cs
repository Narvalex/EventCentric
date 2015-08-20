using EventCentric.Messaging;

namespace EventCentric.Queueing
{
    public class MessageQueue : FSM
    {
        public MessageQueue(IBus bus)
            : base(bus)
        {

        }
    }
}
