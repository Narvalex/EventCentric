using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using Occ.Messages;

namespace Occ.Client.Shared
{
    public class ItemClientHandler : HandlerOf<ItemClient>,
        IHandles<CreateNewItem>,
        IHandles<NewItemCreated>
    {
        public ItemClientHandler(IBus bus, ILogger log, IEventStore<ItemClient> store) : base(bus, log, store)
        {
        }

        public IMessageHandling Handle(NewItemCreated e)
        {
            return base.FromNewStreamIfNotExists(e.ItemId, state =>
            state.UpdateIf(state.StateOfTheItem == ItemClientState.WaitingServerAproval,
                new ItemCreationWasAcceptedByTheServer(e.ItemId))
            .Update(new NewItemCreated(e.ItemId, e.Name)));
        }

        public IMessageHandling Handle(CreateNewItem command)
        {
            return base.FromNewStream(command.ItemId, state =>
            state.Update(new NewItemNeedsToBeAcceptedByTheServer(command.ItemId, command.Name)));
        }
    }
}
