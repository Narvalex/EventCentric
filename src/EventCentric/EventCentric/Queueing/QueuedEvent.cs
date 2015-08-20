using EventCentric.EventSourcing;

namespace EventCentric.Queueing
{
    public class QueuedEvent : Event
    {
        public QueuedEvent()
        {
            this.StreamType = string.Empty;
            this.Version = default(int);
        }
    }
}
