using EventCentric.EntityFramework;
using EventCentric.EntityFramework.Mapping;
using EventCentric.EventSourcing;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Processing
{
    public class SubscriptionWriter : ISubscriptionWriter
    {
        private readonly Func<SubscriptionDbContext> contextFactory;
        private readonly ITimeProvider time;
        private readonly ITextSerializer serializer;

        public SubscriptionWriter(Func<SubscriptionDbContext> contextFactory, ITimeProvider time, ITextSerializer serializer)
        {
            this.contextFactory = contextFactory;
            this.time = time;
            this.serializer = serializer;
        }

        public void LogIncomingEventAsReceivedAndIgnored(IEvent @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                var message = new InboxEntity
                {
                    EventId = @event.EventId,
                    StreamType = @event.StreamType,
                    StreamId = @event.StreamId,
                    Version = @event.Version,
                    EventType = @event.GetType().Name,
                    CreationDate = time.Now,
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
                        subscription
                    });
            }
        }
    }
}
