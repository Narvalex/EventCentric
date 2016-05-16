using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using PingPong.Common.Messages.Client;

namespace PingPong.Client.Domain
{
    public class PlayerHandler : HandlerOf<Player>,
        IHandles<RegisterPlayerAndStartPlaying>
    {
        public PlayerHandler(IBus bus, ILogger log, IEventStore<Player> store) : base(bus, log, store)
        {
        }

        public IMessageHandling Handle(RegisterPlayerAndStartPlaying command)
        {
            return this.FromNewStream(command.PlayerId, state =>
            state.Update(
                new NewPlayerRegistered(),
                new CreateOrJoinAMatch()));
        }
    }
}
