using EventCentric.EventSourcing;
using System;

namespace SimplePingPong.Domain
{
    public class HitBall : Command
    {
        public HitBall(Guid gameId)
        {
            this.GameId = gameId;
        }

        public Guid GameId { get; }
    }
}
