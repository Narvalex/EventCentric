﻿using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using System;
using System.Collections.Generic;

namespace EventCentric.Tests.Specifications
{
    public class InventoryHandler : HandlerOf<Inventory>,
        IHandles<AddItem>
    {
        public InventoryHandler(ISystemBus bus, ILogger log, IEventStore<Inventory> store) : base(bus, log, store) { }

        public IMessageHandling Handle(AddItem command)
            => base.FromNewStreamIfNotExists(command.InventoryId, aggregate => aggregate.Handle(command));
    }


    public class Inventory : StateOf<Inventory>,
        IUpdatesWhen<NewCollectionOfItems>,
        IUpdatesWhen<ItemsAdded>
    {
        public Inventory(Guid id) : base(id) { }

        public Inventory(Guid id, ISnapshot snapshot) : base(id, snapshot) { }

        public Inventory(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents) { }

        public Dictionary<Guid, int> ItemsQuantityById { get; private set; } = new Dictionary<Guid, int>();

        public void When(ItemsAdded e) => this.ItemsQuantityById[e.ItemId] = e.Quantity;

        public void When(NewCollectionOfItems e) => this.ItemsQuantityById.Add(e.ItemId, e.Quantity);
    }

    public static class InventoryExtensions
    {
        public static Inventory Handle(this Inventory s, AddItem command)
            => s.ItemsQuantityById.ContainsKey(command.ItemId)
                        ? s.Update(new ItemsAdded(command.ItemId, command.Quantity))
                        : s.Update(new NewCollectionOfItems(command.ItemId, command.Quantity));
    }

    public class AddItem : Event
    {
        public AddItem(Guid inventoryId, Guid itemId, int quantity)
        {
            this.InventoryId = inventoryId;
            this.ItemId = itemId;
            this.Quantity = quantity;
        }

        public Guid InventoryId { get; }
        public Guid ItemId { get; }
        public int Quantity { get; }
    }

    public class NewCollectionOfItems : Event
    {
        public NewCollectionOfItems(Guid itemId, int quantity)
        {
            this.ItemId = itemId;
            this.Quantity = quantity;
        }

        public Guid ItemId { get; }
        public int Quantity { get; }
    }

    public class ItemsAdded : Event
    {
        public ItemsAdded(Guid itemId, int quantity)
        {
            this.ItemId = itemId;
            this.Quantity = quantity;
        }

        public Guid ItemId { get; }
        public int Quantity { get; }
    }
}
