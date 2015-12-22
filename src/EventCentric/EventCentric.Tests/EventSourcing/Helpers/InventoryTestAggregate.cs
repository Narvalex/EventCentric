using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;

namespace EventCentric.Tests.EventSourcing.Helpers
{
    public class InventoryTestAggregate : EventSourced,
        IHandles<AddItems>,
        IUpdatesOn<ItemsAdded>
    {
        private int quantity;

        public InventoryTestAggregate(Guid id) : base(id) { }

        public InventoryTestAggregate(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public InventoryTestAggregate(Guid id, IMemento memento)
            : base(id, memento)
        {
            var state = ((InventoryTestAggregateMemento)memento);
            this.quantity = state.Quantity;
        }

        public override IMemento SaveToMemento()
        {
            return new InventoryTestAggregateMemento(this.Version, this.quantity);
        }

        public void Handle(AddItems command)
        {
            base.Update(new ItemsAdded(command.Quantity));
        }

        protected override void OnUpdateStarted()
        {
            this.quantity = 0;
        }

        public void On(ItemsAdded e)
        {
            this.quantity = e.Quantity;
        }
    }

    public class InventoryTestAggregateMemento : Memento
    {
        public InventoryTestAggregateMemento(long version, int quantity)
            : base(version)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; }
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

    public class ItemsAdded : Event
    {
        public ItemsAdded(int quantity)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; }
    }
    #endregion
}
