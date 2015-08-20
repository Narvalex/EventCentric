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
                finder: () => context
                                    .Subscriptions
                                    .Where(s => s.StreamId == @event.StreamId && s.StreamType == @event.StreamType)
                                    .SingleOrDefault(),
                add: () => new SubscriptionEntity
                {
                    StreamType = @event.StreamType,
                    StreamId = @event.StreamId,
                    // There must be at least one subscription in order to perform correctly.
                    Url = context.Subscriptions.Where(s => s.StreamType == @event.StreamType).First().Url,
                    LastProcessedVersion = @event.Version,
                    LastProcessedEventId = @event.EventId,
                    CreationDate = this.time.Now,
                    IsPoisoned = false
                },
                update: (subscription) =>
                {
                    subscription.LastProcessedVersion = @event.Version;
                    subscription.LastProcessedEventId = @event.EventId;
                });
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
