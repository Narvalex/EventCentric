using EventCentric.EventSourcing;
using System;
using System.Collections.Generic;

namespace EventCentric.Tests.EventSourcing.Helpers
{
    public class CartTestAggregate : EventSourced,
        IHandles<CreateCart>,
        IHandles<AddItems>,
        IHandles<RemoveItems>,
        IUpdatesOn<CartCreated>,
        IUpdatesOn<ItemsAdded>,
        IUpdatesOn<ItemsRemoved>
    {
        private int itemsInCartCount = 0;

        public CartTestAggregate(Guid id)
            : base(id)
        { }

        public CartTestAggregate(Guid id, IEnumerable<IEvent> streamOfEvents)
            : base(id, streamOfEvents)
        { }

        public CartTestAggregate(Guid id, IMemento memento)
            : base(id, memento)
        {
            var state = (CartMemento)memento;
            this.itemsInCartCount = state.ItemsInCartCount;
        }

        public void Handle(CreateCart e)
        {
            base.Update(new CartCreated(e.CartId));
        }

        public void Handle(AddItems e)
        {
            base.Update(new ItemsAdded(e.Quantity));
        }

        public void Handle(RemoveItems e)
        {
            base.Update(new ItemsRemoved(e.Quantity));
        }

        public void On(ItemsRemoved e)
        {
            this.itemsInCartCount -= e.Quantity;
        }

        public void On(ItemsAdded e)
        {
            this.itemsInCartCount += e.Quantity;
        }

        public void On(CartCreated e)
        { }

        public override IMemento SaveToMemento()
        {
            return new CartMemento(base.Version, this.itemsInCartCount);
        }
    }

    public class CartMemento : Memento
    {
        public CartMemento(int version,
            int itemsInCartCount)
            : base(version)
        {
            this.ItemsInCartCount = itemsInCartCount;
        }

        public int ItemsInCartCount { get; private set; }
    }
}
