using EventCentric.EventSourcing;
using System;

namespace PingPong.Common.Messages.Client
{
    public class RegisterPlayerAndStartPlaying : Command
    {
        public RegisterPlayerAndStartPlaying(Guid playerId)
        {
            this.PlayerId = playerId;
        }

        public Guid PlayerId { get; }
    }
}
