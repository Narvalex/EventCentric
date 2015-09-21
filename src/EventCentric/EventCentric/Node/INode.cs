namespace EventCentric
{
    public interface INode
    {
        string Name { get; }

        void Start();

        void Stop();

        NodeState State { get; }
    }
}
