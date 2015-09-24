using EventCentric.EventSourcing;
using EventCentric.Queueing;
using System;

namespace EventCentric.Tests.EventSourcing.Helpers
{
    #region Queue

    public class CreateCart : QueuedEvent
    {
        public CreateCart(Guid streamId, Guid transactionId, Guid eventId,
            Guid cartId)
            : base(streamId, transactionId)
        {
            this.CartId = cartId;
            this.EventId = eventId;
        }

        public Guid CartId { get; private set; }
    }

    public class AddItems : QueuedEvent
    {
        public AddItems(Guid streamId, Guid transactionId, Guid eventId,
            Guid cartId, int quantity)
            : base(streamId, transactionId)
        {
            this.CartId = cartId;
            this.Quantity = quantity;
            this.EventId = eventId;
        }

        public Guid CartId { get; private set; }
        public int Quantity { get; private set; }
    }

    public class RemoveItems : QueuedEvent
    {
        public RemoveItems(Guid streamId, Guid transactionId, Guid eventId,
            Guid cartId, int quantity)
            : base(streamId, transactionId)
        {
            this.CartId = cartId;
            this.Quantity = quantity;
            this.EventId = eventId;
        }

        public Guid CartId { get; private set; }
        public int Quantity { get; private set; }
    }

    #endregion

    #region Saga

    public class CartCreated : Event
    {
        public CartCreated(Guid cartId)
        {
            this.CartId = cartId;
        }

        public Guid CartId { get; private set; }
    }

    public class ItemsAdded : Event
    {
        public ItemsAdded(int quantity)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; private set; }
    }

    public class ItemsRemoved : Event
    {
        public ItemsRemoved(int quantity)
        {
            this.Quantity = quantity;
        }

        public int Quantity { get; private set; }
    }

    #endregion
}
