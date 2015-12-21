using EventCentric.EventSourcing;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Linq;

namespace EventCentric.Messaging
{
    public class EventQueue : Worker, IEventQueue
    {
        protected readonly string streamType;
        protected readonly Func<bool, IEventQueueDbContext> contextFactory;
        protected readonly ITextSerializer serializer;
        protected readonly IUtcTimeProvider time;
        protected readonly IGuidProvider guid;

        public EventQueue(string streamType, Func<bool, IEventQueueDbContext> contextFactory, ITextSerializer serializer, IUtcTimeProvider time, IGuidProvider guid, IBus bus)
            : base(bus)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, "streamType");
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(time, "time");
            Ensure.NotNull(guid, "guid");

            this.streamType = streamType;
            this.contextFactory = contextFactory;
            this.serializer = serializer;
            this.time = time;
            this.guid = guid;
        }

        public void Enqueue(IEvent @event)
        {
            using (var context = this.contextFactory(false))
            {
                var versions = context.Events
                                      .Where(e => e.StreamId == @event.StreamId)
                                      .AsCachedAnyEnumerable();

                long currentVersion = 0;
                if (versions.Any())
                    currentVersion = versions.Max(e => e.Version);

                var updatedVersion = currentVersion + 1;

                var now = this.time.Now;
                var localNow = now.ToLocalTime();

                @event.AsQueuedEvent(this.streamType, this.guid.NewGuid(), updatedVersion, now, localNow);

                context.Events.Add(
                    new EventEntity
                    {
                        StreamId = @event.StreamId,
                        Version = @event.Version,
                        EventId = @event.EventId,
                        TransactionId = @event.TransactionId,
                        EventType = @event.GetType().Name,
                        LocalTime = localNow,
                        UtcTime = now,
                        Payload = this.serializer.Serialize(@event)
                    });

                context.SaveChanges();

                var version = context.Events.Max(e => e.EventCollectionVersion);

                this.bus.Publish(new EventStoreHasBeenUpdated(version));
            }
        }
    }
}
