using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;

namespace PingPong.Client.Domain
{
    public class Player : StateOf<Player>
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
}
