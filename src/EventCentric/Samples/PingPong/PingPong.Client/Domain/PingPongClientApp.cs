using EventCentric;
using EventCentric.Log;
using EventCentric.Utils;
using PingPong.Common.Messages.Client;
using System;

namespace PingPong.Client.Domain
{
    public class PingPongClientApp : ApplicationService
    {
        public PingPongClientApp(IGuidProvider guid, ILogger log, string streamType, int eventsToPushMaxCount) : base(guid, log, streamType, eventsToPushMaxCount)
        {
        }

        public void StartTest(int playersCount)
        {
            for (int i = 0; i < playersCount; i++)
            {
                var playerId = Guid.NewGuid();
                this.Send(playerId, new RegisterPlayerAndStartPlaying(playerId));
            }
        }
    }
}
