namespace EventCentric.Messaging.Events
{
    public struct FatalErrorOcurred : IMessage
    {
        public FatalErrorOcurred(FatalErrorException exception)
        {
            this.Exception = exception;
        }

        public FatalErrorException Exception { get; }
    }
}
