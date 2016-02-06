using EventCentric.EventSourcing;
using EventCentric.Handling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Tests.EventSourcing.Helpers
{
    public class InventoryTestAggregate : EventSourced,
        IHandles<AddItems>,
        IHandles<RegisterItemName>,
        IUpdatesOn<ItemsAdded>,
        IUpdatesOn<ItemNameRegistered>
    {
        private int quantity;
        private List<string> names = new List<string>();

        public InventoryTestAggregate(Guid id) : base(id) { }

        public InventoryTestAggregate(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public InventoryTestAggregate(Guid id, ISnapshot memento)
            : base(id, memento)
        {
            var state = ((InventoryTestAggregateMemento)memento);
            this.quantity = state.Quantity;
            this.names = state.Names.ToList();
        }

        public override ISnapshot SaveToSnapshot()
        {
            return new InventoryTestAggregateMemento(this.Version, this.quantity, this.names.ToArray());
        }

        public void Handle(AddItems command)
        {
            base.Update(new ItemsAdded(command.Quantity));
        }

        public void Handle(RegisterItemName command)
        {
            base.Update(new ItemNameRegistered(command.Name));
        }

        protected override void OnUpdateStarted()
        {
            this.quantity = 0;
        }

        public void On(ItemsAdded e)
        {
            this.quantity = e.Quantity;
        }

        public void On(ItemNameRegistered e)
        {
            this.names.Add(e.Name);
        }
    }

    public class InventoryTestAggregateMemento : Snapshot
    {
        public InventoryTestAggregateMemento(long version, int quantity, string[] names)
            : base(version)
        {
            this.Quantity = quantity;
            this.Names = names;
        }

        public int Quantity { get; }
        public string[] Names { get; }
    }

    #region Messages
    public class AddItems : Event
    {
        public AddItems(int quantity)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; }
    }

    public class RegisterItemName : Event
    {
        public RegisterItemName(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }

    public class ItemsAdded : Event
    {
        public ItemsAdded(int quantity)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; }
    }

    public class ItemNameRegistered : Event
    {
        public ItemNameRegistered(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
    #endregion
}
