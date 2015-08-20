namespace EventCentric
{
    public interface INode
    {
        void Start();

        void Stop();

        NodeState State { get; }
    }
}
