using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using System;
using System.Collections.Generic;

namespace SimplePingPong.Domain
{
    public class Player : State<Player>
    {
        public Player(Guid id) : base(id)
        {
        }

        public Player(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents)
        {
        }

        public Player(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
        }
    }

    public class PlayerHandler : Handler<Player>, IHandle<HitBall>
    {
        public PlayerHandler(IBus bus, ILogger log, IEventStore<Player> store) : base(bus, log, store)
        {
        }

        public IMessageHandling Handle(HitBall command)
        {
            return base.FromNewStreamIfNotExists(command.GameId, state =>
            state.UpdateAfterSending(new HitBall(command.GameId)));
        }
    }
}
