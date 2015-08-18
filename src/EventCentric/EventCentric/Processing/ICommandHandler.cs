namespace EventCentric.Processing
{
    public interface ICommandHandler
    { }

    public interface ICommandHandler<T> where T : ICommand
    {
        void Handle(T command);
    }
}
