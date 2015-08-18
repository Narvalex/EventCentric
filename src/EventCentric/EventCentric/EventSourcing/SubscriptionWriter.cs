using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.EventSourcing
{
    public class SubscriptionWriter : ISubscriptionWriter
    {
        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly ITimeProvider time;
        private readonly ITextSerializer serializer;

        public SubscriptionWriter(Func<EventStoreDbContext> contextFactory, ITimeProvider time, ITextSerializer serializer)
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
                entityFinder: () => context
                                    .Subscriptions
                                    .Where(s => s.StreamId == @event.StreamId && s.StreamType == @event.StreamType)
                                    .SingleOrDefault(),
                newEntityToAdd: new SubscriptionEntity
                {
                    StreamType = @event.StreamType,
                    StreamId = @event.StreamId,
                    Url = context.Subscriptions.Where(s => s.StreamType == @event.StreamType).First().Url,
                    LastProcessedVersion = @event.Version,
                    LastProcessedEventId = @event.EventId,
                    CreationDate = this.time.Now,
                    IsPoisoned = false
                },
                updateEntity: (subscription) =>
                {
                    subscription.LastProcessedVersion = @event.Version;
                    subscription.LastProcessedEventId = @event.EventId;
                    return subscription;
                });
        }

        public void MarkEventAsPosisoned(IEvent @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                context.AddOrUpdate(
                    entityFinder: () => context
                                        .Subscriptions
                                        .Where(s => s.StreamType == @event.StreamType && s.StreamId == @event.StreamId)
                                        .Single(),
                    newEntityToAdd: null,
                    updateEntity: subscription =>
                    {
                        subscription.LastProcessedEventId = @event.EventId;
                        subscription.LastProcessedVersion = @event.Version;
                        subscription.IsPoisoned = true;
                        return subscription;
                    });

                context.SaveChanges();
            }
        }
    }
}
