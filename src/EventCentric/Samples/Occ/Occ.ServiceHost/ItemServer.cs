using EventCentric.EventSourcing;
using Occ.Messages;
using System;
using System.Collections.Generic;

namespace Occ.Server
{
    public class ItemServer : StateOf<ItemServer>,
        IUpdatesWhen<NewItemCreated>
    {
        public ItemServer(Guid id) : base(id)
        {
        }

        public ItemServer(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents)
        {
        }

        public ItemServer(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
        }

        public void When(NewItemCreated e) { }
    }
}
