using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.EventSourcing
{
    public class SubscriptionInboxWriter : ISubscriptionInboxWriter
    {
        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly ITimeProvider time;
        private readonly ITextSerializer serializer;

        public SubscriptionInboxWriter(Func<EventStoreDbContext> contextFactory, ITimeProvider time, ITextSerializer serializer)
        {
            this.contextFactory = contextFactory;
            this.time = time;
            this.serializer = serializer;
        }

        public void LogIncomingEventAsIgnored(IEvent @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                this.LogIncomingEvent(@event, context, true);
                context.SaveChanges();
            }
        }

        public void LogIncomingEvent(IEvent @event, EventStoreDbContext context, bool ignored = false)
        {
            var message = new InboxEntity
            {
                EventId = @event.EventId,
                StreamType = @event.StreamType,
                StreamId = @event.StreamId,
                Version = @event.Version,
                EventType = @event.GetType().Name,
                CreationDate = time.Now,
                Ignored = ignored,
                Payload = this.serializer.Serialize(@event)
            };
            context.Inbox.Add(message);

            context.AddOrUpdate(
                finder: () => context
                                    .Subscriptions
                                    .Where(s => s.StreamId == @event.StreamId && s.StreamType == @event.StreamType)
                                    .SingleOrDefault(),
                add: () => { throw new InvalidOperationException("Subscription does not exist!"); },
                update: subscription =>
                {
                    subscription.LastProcessedVersion = @event.Version;
                    subscription.LastProcessedEventId = @event.EventId;
                });
        }

        public void CreateNewSubscription(string streamType, Guid streamId, int updatedStreamCollectionVersion)
        {
            using (var context = this.contextFactory())
            {
                var now = this.time.Now;

                // Updating the source table
                context.AddOrUpdate(
                    // there must be a subscribed source to create a subscription
                    finder: () => context.SubscribedSources.Where(s => s.StreamType == streamType).Single(),
                    add: () => { throw new InvalidOperationException("Subscribed source does not exist!"); },
                    update: source => source.StreamCollectionVersion = updatedStreamCollectionVersion > source.StreamCollectionVersion ? updatedStreamCollectionVersion : source.StreamCollectionVersion);

                // Creating a new entry in the subscription table
                var subscription = new SubscriptionEntity
                {
                    StreamType = streamType,
                    StreamId = streamId,
                    LastProcessedEventId = Guid.Empty,
                    LastProcessedVersion = 0,
                    CreationDate = now,
                    IsPoisoned = false
                };
            }
        }

        public void LogPosisonedMessage(string streamType, Guid streamId, PoisonMessageException exception)
        {
            using (var context = this.contextFactory.Invoke())
            {
                context.AddOrUpdate(
                    finder: () => context
                                    .Subscriptions
                                    .Where(s => s.StreamType == streamType && s.StreamId == streamId)
                                    .Single(),
                    add: null,
                    update: subscription =>
                    {
                        subscription.IsPoisoned = true;
                        subscription.ExceptionMessage = this.serializer.Serialize(exception);
                    });

                context.SaveChanges();
            }
        }
    }
}
