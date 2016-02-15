namespace EventCentric
{
    public interface IMicroservice
    {
        string Name { get; }

        void Start();

        void Stop();

        WorkerStatus Status { get; }
    }
}
