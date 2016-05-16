using EventCentric.EventSourcing;

namespace PersistenceBenchmark.PromotionsStream
{
    public class PromotionsSnapshot : Snapshot
    {
        public PromotionsSnapshot(long version, int points) : base(version)
        {
            this.Points = points;
        }

        public int Points { get; }
    }
}
