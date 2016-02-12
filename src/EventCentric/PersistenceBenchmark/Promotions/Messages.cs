using EventCentric.EventSourcing;
using System;
namespace PersistenceBenchmark.PromotionsStream
{
    public class FreePointsRewardedToUser : Event
    {
        public FreePointsRewardedToUser(Guid userId, int points)
        {
            this.UserId = userId;
            this.Points = points;
        }

        public Guid UserId { get; }
        public int Points { get; }
    }
}
