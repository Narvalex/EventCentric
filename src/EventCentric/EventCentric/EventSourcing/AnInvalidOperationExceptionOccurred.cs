namespace EventCentric.EventSourcing
{
    public class AnInvalidOperationExceptionOccurred : Event
    {
        public AnInvalidOperationExceptionOccurred(string message)
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}
