using EventCentric.EventSourcing;
using System;

namespace Occ.Messages
{
    public class CreateNewItem : Command
    {
        public CreateNewItem(Guid itemId, string name)
        {
            this.ItemId = itemId;
            this.Name = name;
        }

        public Guid ItemId { get; }
        public string Name { get; }
    }
}
