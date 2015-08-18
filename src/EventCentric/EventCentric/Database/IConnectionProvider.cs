namespace EventCentric.Database
{
    public interface IConnectionProvider
    {
        string ConnectionString { get; }
    }
}
