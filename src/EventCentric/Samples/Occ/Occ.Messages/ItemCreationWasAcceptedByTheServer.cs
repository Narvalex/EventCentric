using EventCentric.EventSourcing;
using System;

namespace Occ.Messages
{
    public class ItemCreationWasAcceptedByTheServer : Event
    {
        public ItemCreationWasAcceptedByTheServer(Guid itemId)
        {
            this.ItemId = itemId;
        }

        public Guid ItemId { get; }
    }
}
