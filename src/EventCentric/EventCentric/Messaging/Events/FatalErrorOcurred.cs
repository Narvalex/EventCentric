namespace EventCentric.Messaging.Events
{
    public class FatalErrorOcurred : IMessage
    {
        public FatalErrorOcurred(FatalErrorException exception)
        {
            this.Exception = exception;
        }

        public FatalErrorException Exception { get; private set; }
    }
}
