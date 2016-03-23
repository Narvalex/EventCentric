using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using Occ.Messages;

namespace Occ.Server
{
    public class ItemServerHandler : HandlerOf<ItemServer>,
        IHandles<NewItemNeedsToBeAcceptedByTheServer>
    {
        public ItemServerHandler(IBus bus, ILogger log, IEventStore<ItemServer> store) : base(bus, log, store)
        {
        }

        public IMessageHandling Handle(NewItemNeedsToBeAcceptedByTheServer e)
        {
            return base.FromNewStream(e.ItemId, state => state.Update(new NewItemCreated(e.ItemId, e.Name)));
        }
    }
}
