using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using System;

namespace PersistenceBenchmark.PromotionsStream
{
    public class PromotionsHandler : HandlerOf<Promotions>,
        IHandle<UserCreatedOrUpdated>,
        IHandle<TryAddNewSubscription>
    {
        public PromotionsHandler(IBus bus, ILogger log, IEventStore<Promotions> store) : base(bus, log, store) { }

        public IMessageHandling Handle(TryAddNewSubscription message)
        {
            return base.FromNewStreamIfNotExists(message.StreamId, s =>
            s.Tee(x =>
            {
                this.bus.Publish(new AddNewSubscriptionOnTheFly(message.StreamTypeOfProducer, message.Url, message.Token));
                //this.bus.Publish(new AddNewSubscriptionOnTheFly(message.StreamTypeOfProducer, message.Url, message.Token));
            })
            .Update(new NewSubscriptionAdded()));
        }

        public IMessageHandling Handle(UserCreatedOrUpdated e) =>
            base.FromNewStreamIfNotExists(e.UserId, state =>
            state.Update(new FreePointsRewardedToUser(state.Id, 1)));
    }
}
