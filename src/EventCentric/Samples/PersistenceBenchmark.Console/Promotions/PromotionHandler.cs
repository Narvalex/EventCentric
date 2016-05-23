using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;

namespace PersistenceBenchmark.PromotionsStream
{
    public class PromotionsHandler : HandlerOf<Promotions>,
        IHandle<UserCreatedOrUpdated>
    {
        public PromotionsHandler(IBus bus, ILogger log, IEventStore<Promotions> store) : base(bus, log, store) { }

        public IMessageHandling Handle(UserCreatedOrUpdated e) =>
            base.FromNewStreamIfNotExists(e.UserId, state =>
            state.Update(new FreePointsRewardedToUser(state.Id, 1)));
    }
}
