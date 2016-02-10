namespace EventCentric.EventSourcing
{
    public class Command : Message
    {
        public Command() { this.IsACommand = true; }
    }
}
