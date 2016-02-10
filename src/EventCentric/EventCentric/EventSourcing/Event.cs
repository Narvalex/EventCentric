namespace EventCentric.EventSourcing
{
    public class Event : Message
    {
        public Event() { this.IsACommand = false; }
    }
}
