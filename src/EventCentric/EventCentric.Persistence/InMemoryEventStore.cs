using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace EventCentric.Persistence
{
    public class InMemoryEventStore<T> :
        ISubscriptionRepository,
        IEventDao,
        IEventStore<T> where T : class, IEventSourced
    {
        private long eventCollectionVersion = 0;
        private readonly string streamType;
        private readonly IUtcTimeProvider time;
        private readonly ITextSerializer serializer;
        private readonly IGuidProvider guid;
        private readonly ILogger log;
        private readonly ObjectCache cache;
        private readonly Func<Guid, IEnumerable<IEvent>, T> aggregateFactory;
        private readonly Func<Guid, ISnapshot, T> originatorAggregateFactory;

        private readonly ConcurrentBag<EventEntity> Events = new ConcurrentBag<EventEntity>();
        private readonly ConcurrentBag<SubscriptionEntity> Subscriptions = new ConcurrentBag<SubscriptionEntity>();
        private readonly ConcurrentBag<SnapshotEntity> Snapshots = new ConcurrentBag<SnapshotEntity>();
        private readonly ConcurrentBag<InboxEntity> Inbox = new ConcurrentBag<InboxEntity>();

        public InMemoryEventStore(string microserviceName, IUtcTimeProvider time, ITextSerializer serializer, IGuidProvider guid, ILogger log)
        {
            Ensure.NotNull(microserviceName, nameof(microserviceName));
            Ensure.NotNull(time, nameof(time));
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));

            this.streamType = microserviceName;
            this.time = time;
            this.serializer = serializer;
            this.guid = guid;
            this.log = log;
            this.cache = new MemoryCache(microserviceName);

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var fromMementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(ISnapshot) });
            Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
            this.originatorAggregateFactory = (id, memento) => (T)fromMementoConstructor.Invoke(new object[] { id, memento });

            var fromStreamConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
            Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
            this.aggregateFactory = (id, streamOfEvents) => (T)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });

            if (this.Events.Any(e => e.StreamType == this.streamType))
                this.eventCollectionVersion = this.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);
        }

        public void Setup(IEnumerable<SubscriptionEntity> subscriptions)
        {
            subscriptions.ForEach(s => this.Subscriptions.Add(s));
        }

        public EventStoreStats GetStats()
        {
            return new EventStoreStats(this.Events.Count(), this.Inbox.Count());
        }

        #region EventDao
        public List<NewRawEvent> FindEvents(long fromEventCollectionVersion, int quantity)
        {
            return this.Events
                        .Where(e => e.StreamType == this.streamType && e.EventCollectionVersion > fromEventCollectionVersion)
                        .OrderBy(e => e.EventCollectionVersion)
                        .Take(quantity)
                        .Select(e => new NewRawEvent(e.EventCollectionVersion, e.Payload))
                        .ToList();
        }

        public long GetEventCollectionVersion()
        {
            return !this.Events.Any(e => e.StreamType == this.streamType)
                    ? 0
                    : this.Events.Where(e => e.StreamType == this.streamType)
                        .Max(e => e.EventCollectionVersion);
        }
        #endregion

        #region SubscriptionRepository
        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            var sub = this.Subscriptions.Where(s => s.StreamType == poisonedEvent.StreamType && s.SubscriberStreamType == this.streamType).Single();
            sub.IsPoisoned = true;
            sub.UpdateLocalTime = this.time.Now.ToLocalTime();
            sub.PoisonEventCollectionVersion = poisonedEvent.EventCollectionVersion;
            sub.ExceptionMessage = this.serializer.Serialize(exception);
            sub.DeadLetterPayload = this.serializer.Serialize(poisonedEvent);
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            return this.Subscriptions
                        .Where(s => s.SubscriberStreamType == this.streamType && !s.IsPoisoned && !s.WasCanceled)
                        .Select(s => new SubscriptionBuffer(s.StreamType.Trim(), s.Url.Trim(), s.Token, s.ProcessorBufferVersion - 1, s.IsPoisoned))
                        .ToArray();
        }
        #endregion

        #region EventStore
        public T Find(Guid id)
        {
            // get memento from cache
            var cachedMemento = (Tuple<ISnapshot, DateTime?>)this.cache.Get(id.ToString());
            if (cachedMemento == null || !cachedMemento.Item2.HasValue)
            {
                var snapshotEntity = this.Snapshots.Where(s => s.StreamId == id && s.StreamType == this.streamType).SingleOrDefault();
                if (snapshotEntity != null)
                    cachedMemento = new Tuple<ISnapshot, DateTime?>(this.serializer.Deserialize<ISnapshot>(snapshotEntity.Payload), null);
                else
                    return this.GetFromFullStreamOfEvents(id);
            }

            return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
        }

        private T GetFromFullStreamOfEvents(Guid id)
        {
            var stream = this.Events
                                .Where(e => e.StreamId == id && e.StreamType == this.streamType)
                                .OrderBy(e => e.Version)
                                .AsEnumerable()
                                .Select(e => this.serializer.Deserialize<IEvent>(e.Payload))
                                .AsCachedAnyEnumerable();

            if (stream.Any())
                return this.aggregateFactory.Invoke(id, stream);

            return null;
        }

        public T Get(Guid id)
        {
            var aggregate = this.Find(id);
            if (aggregate == null)
            {
                var ex = new StreamNotFoundException(id, streamType);
                this.log.Error(ex, string.Format("Stream not found exception for stream {0} with id of {1}", streamType, id));
                throw ex;
            }

            return aggregate;
        }

        public long Save(T eventSourced, IEvent incomingEvent)
        {
            var pendingEvents = eventSourced.PendingEvents;
            if (pendingEvents.Count == 0)
                return this.eventCollectionVersion;

            if (eventSourced.Id == default(Guid))
                throw new ArgumentOutOfRangeException("StreamId", $"The eventsourced of type {typeof(T).FullName} has a default GUID value for its stream id, which is not valid");

            var key = eventSourced.Id.ToString();
            try
            {
                var currentVersion = this.Events.Any(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamType)
                                          ? this.Events
                                            .Where(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamType)
                                            .Max(e => e.Version)
                                          : 0;

                // Check if incoming event is duplicate
                if (this.IsDuplicate(incomingEvent.EventId))
                    // Incoming event is duplicate
                    return currentVersion;

                if (currentVersion + 1 != pendingEvents.First().Version)
                    throw new EventStoreConcurrencyException();

                var now = this.time.Now;
                var localNow = this.time.Now.ToLocalTime();

                // Log the incoming message in the inbox
                var message = new InboxEntity
                {
                    InboxStreamType = this.streamType,
                    EventId = incomingEvent.EventId,
                    TransactionId = incomingEvent.TransactionId,
                    StreamType = incomingEvent.StreamType,
                    StreamId = incomingEvent.StreamId,
                    Version = incomingEvent.Version,
                    EventType = incomingEvent.GetType().Name,
                    EventCollectionVersion = incomingEvent.EventCollectionVersion,
                    LocalTime = localNow,
                    Ignored = false,
                    Payload = this.serializer.Serialize(incomingEvent)
                };
                this.Inbox.Add(message);

                // Update subscription
                try
                {
                    var subscription = this.Subscriptions.Where(s => s.StreamType == incomingEvent.StreamType && s.SubscriberStreamType == this.streamType).Single();
                    subscription.ProcessorBufferVersion = incomingEvent.ProcessorBufferVersion;
                    subscription.UpdateLocalTime = now;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"The incoming event belongs to a an stream type of {incomingEvent.StreamType}, but the event store could not found a subscription for that stream type.", ex);
                }


                // Cache Memento And Publish Stream
                var snapshot = ((ISnapshotOriginator)eventSourced).SaveToSnapshot();

                // Cache in Sql Server
                var serializedMemento = this.serializer.Serialize(snapshot);

                var streamEntity = this.Snapshots.Where(s => s.StreamId == eventSourced.Id && s.StreamType == this.streamType).SingleOrDefault();
                if (streamEntity != null)
                {
                    streamEntity.Version = eventSourced.Version;
                    streamEntity.Payload = serializedMemento;
                    streamEntity.UpdateLocalTime = localNow;
                }
                else
                {
                    streamEntity = new SnapshotEntity
                    {
                        StreamType = this.streamType,
                        StreamId = eventSourced.Id,
                        Version = eventSourced.Version,
                        Payload = serializedMemento,
                        CreationLocalTime = localNow,
                        UpdateLocalTime = localNow
                    };
                    this.Snapshots.Add(streamEntity);
                }

                // Cache in memory
                this.cache.Set(
                    key: key,
                    value: new Tuple<ISnapshot, DateTime?>(snapshot, now),
                    policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });


                List<EventEntity> eventEntities = new List<EventEntity>();
                foreach (var @event in pendingEvents)
                {
                    var e = (Message)@event;
                    e.TransactionId = incomingEvent.TransactionId;
                    e.EventId = this.guid.NewGuid();
                    e.StreamType = this.streamType;
                    e.LocalTime = now;
                    e.UtcTime = localNow;

                    eventEntities.Add(
                        new EventEntity
                        {
                            StreamType = this.streamType,
                            StreamId = @event.StreamId,
                            Version = @event.Version,
                            EventId = @event.EventId,
                            TransactionId = @event.TransactionId,
                            EventType = @event.GetType().Name,
                            CorrelationId = incomingEvent.EventId,
                            LocalTime = localNow,
                            UtcTime = now
                        });
                }

                lock (this)
                {
                    for (int i = 0; i < pendingEvents.Count; i++)
                    {
                        var ecv = Interlocked.Increment(ref this.eventCollectionVersion);
                        var @event = pendingEvents[i];
                        ((Message)@event).EventCollectionVersion = ecv;
                        var entity = eventEntities[i];
                        entity.EventCollectionVersion = ecv;
                        entity.Payload = this.serializer.Serialize(@event);
                        this.Events.Add(entity);
                    }

                    //return context.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);
                    return this.eventCollectionVersion;
                }
            }
            catch (Exception ex)
            {
                this.log.Error(ex, $"An error ocurred while storing events while processing incoming event of type '{incomingEvent.GetType().Name}'");

                // Mark cache as stale
                var item = (Tuple<ISnapshot, DateTime?>)this.cache.Get(key);
                if (item != null && item.Item2.HasValue)
                {
                    item = new Tuple<ISnapshot, DateTime?>(item.Item1, null);
                    this.cache.Set(
                        key,
                        item,
                        new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });
                }

                throw;
            }
        }

        public void DeleteSnapshot(Guid streamId)
        {
            cache.Remove(streamId.ToString());

            SnapshotEntity snapshot;

            while (this.Snapshots.TryTake(out snapshot))
            {
                // this is not optimized, but in-memory persistence is not meant to be production-ready though.
                if (snapshot.StreamId == streamId)
                    break;
            }
        }

        public bool IsDuplicate(Guid eventId)
        {
            return this.Inbox.Any(e => e.EventId == eventId);
        }
        #endregion
    }

    public class EventStoreStats
    {
        public EventStoreStats(int eCount, int iCount)
        {
            this.EventsCount = eCount;
            this.InboxCount = iCount;
        }

        public int EventsCount { get; }
        public int InboxCount { get; }
    }
}
