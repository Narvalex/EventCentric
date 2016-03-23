using EventCentric.EventSourcing;
using Occ.Messages;
using System;
using System.Collections.Generic;

namespace Occ.Client.Shared
{
    public class ItemClient : StateOf<ItemClient>,
        IUpdatesWhen<NewItemNeedsToBeAcceptedByTheServer>,
        IUpdatesWhen<ItemCreationWasAcceptedByTheServer>,
        IUpdatesWhen<NewItemCreated>
    {
        private ItemClientState state = ItemClientState.Undefined;

        public ItemClient(Guid id) : base(id)
        {
        }

        public ItemClient(Guid id, IEnumerable<IEvent> streamOfEvents) : base(id, streamOfEvents)
        {
        }

        public ItemClient(Guid id, ISnapshot snapshot) : base(id, snapshot)
        {
            var state = (ItemClientSnapshot)snapshot;
            this.state = state.State;
        }

        public override ISnapshot SaveToSnapshot()
        {
            return new ItemClientSnapshot(this.Version, this.state);
        }

        public ItemClientState StateOfTheItem => this.state;

        public void When(NewItemNeedsToBeAcceptedByTheServer e) => this.state = ItemClientState.WaitingServerAproval;

        public void When(ItemCreationWasAcceptedByTheServer e) => this.state = ItemClientState.ItemAprovedByServer;

        public void When(NewItemCreated e) => this.state = ItemClientState.Created;
    }

    public enum ItemClientState
    {
        Undefined = 0,
        WaitingServerAproval = 1,
        ItemAprovedByServer = 2,
        Created = 3
    }

    public class ItemClientSnapshot : Snapshot
    {
        public ItemClientSnapshot(long version, ItemClientState state)
            : base(version)
        {
            this.State = state;
        }

        public ItemClientState State { get; }
    }
}
