using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EventCentric.Persistence
{
    public class InMemoryEventStore<T> : ISubscriptionRepository, IEventStore<T> where T : class, IEventSourced
    {
        private long eventCollectionVersion = 0;
        private readonly string streamName;
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
        private readonly Func<IEvent, InboxEntity> inboxEntityFactory;
        private readonly Func<string, string, bool> consumerFilter;

        private readonly object dbLock = new object();

        public InMemoryEventStore(string streamName, IUtcTimeProvider time, ITextSerializer serializer, IGuidProvider guid, ILogger log, bool persistIncomingPayloads, Func<string, string, bool> consumerFilter)
        {
            Ensure.NotNull(streamName, nameof(streamName));
            Ensure.NotNull(time, nameof(time));
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));

            this.streamName = streamName;
            this.time = time;
            this.serializer = serializer;
            this.guid = guid;
            this.log = log;
            this.cache = new MemoryCache(streamName);
            this.consumerFilter = consumerFilter != null ? consumerFilter : EventStoreFuncs.DefaultFilter;

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var fromMementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(ISnapshot) });
            Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
            this.originatorAggregateFactory = (id, memento) => (T)fromMementoConstructor.Invoke(new object[] { id, memento });

            var fromStreamConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
            Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
            this.aggregateFactory = (id, streamOfEvents) => (T)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });

            if (persistIncomingPayloads)
                this.inboxEntityFactory = incomingEvent => new InboxEntity
                {
                    InboxStreamType = this.streamName,
                    EventId = incomingEvent.EventId,
                    Payload = this.serializer.Serialize(incomingEvent),
                    TransactionId = incomingEvent.TransactionId
                };
            else
                this.inboxEntityFactory = incomingEvent => new InboxEntity { InboxStreamType = this.streamName, EventId = incomingEvent.EventId, TransactionId = incomingEvent.TransactionId };

            if (this.Events.Any(e => e.StreamType == this.streamName))
                this.eventCollectionVersion = this.Events.Where(e => e.StreamType == this.streamName).Max(e => e.EventCollectionVersion);
            this.CurrentEventCollectionVersion = this.eventCollectionVersion;

            // Add subscription of app, if not exists
            if (!this.Subscriptions.Any(s => s.SubscriberStreamType == this.streamName && s.StreamType == this.streamName + Constants.AppEventStreamNameSufix))
            {
                // We should add the new subscription
                this.Subscriptions.Add(new SubscriptionEntity
                {
                    SubscriberStreamType = this.streamName,
                    StreamType = this.streamName + Constants.AppEventStreamNameSufix,
                    Url = "none",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = true,
                    CreationLocalTime = DateTime.Now,
                    UpdateLocalTime = DateTime.Now
                });
            }
        }

        public InMemoryEventStore<T> Setup(IEnumerable<SubscriptionEntity> subscriptions)
        {
            subscriptions.ForEach(s => this.Subscriptions.Add(s));
            return this;
        }

        public InMemoryEventStore<T> Setup(params SubscriptionEntity[] subscriptions)
        {
            subscriptions.ForEach(s => this.Subscriptions.Add(s));
            return this;
        }

        public EventStoreStats GetStats()
        {
            return new EventStoreStats(this.Events.Count(), this.Inbox.Count());
        }

        public SerializedEvent[] FindEventsForConsumer(long from, long to, int quantity, string consumer)
        {
            return this.Events
                        .Where(e => e.StreamType == this.streamName
                                    && e.EventCollectionVersion > from
                                    && e.EventCollectionVersion <= to)
                        .OrderBy(e => e.EventCollectionVersion)
                        .Take(quantity)
                        .Select(e =>
                            EventStoreFuncs.ApplyConsumerFilter(
                                new SerializedEvent(e.EventCollectionVersion, e.Payload),
                                consumer,
                                this.serializer,
                                this.consumerFilter))
                        .ToArray();
        }

        public SerializedEvent[] FindEventsForConsumer(long from, long to, Guid streamId, int quantity, string consumer)
        {
            var events = this.Events
                       .Where(e => e.StreamType == this.streamName
                                   && e.EventCollectionVersion > from
                                   && e.EventCollectionVersion <= to
                                   && e.StreamId == streamId)
                       .OrderBy(e => e.EventCollectionVersion)
                       .Take(quantity)
                       .Select(e =>
                           EventStoreFuncs.ApplyConsumerFilter(
                               new SerializedEvent(e.EventCollectionVersion, e.Payload),
                               consumer,
                               this.serializer,
                               this.consumerFilter))
                       .ToArray();

            return events.Length > 0
                ? events
                : new SerializedEvent[] { new SerializedEvent(to, this.serializer.Serialize(new CloakedEvent(to, this.streamName))) };
        }

        public long GetEventCollectionVersion()
        {
            return !this.Events.Any(e => e.StreamType == this.streamName)
                    ? 0
                    : this.Events.Where(e => e.StreamType == this.streamName)
                        .Max(e => e.EventCollectionVersion);
        }

        #region SubscriptionRepository
        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            var sub = this.Subscriptions.Where(s => s.StreamType == poisonedEvent.StreamType && s.SubscriberStreamType == this.streamName).Single();
            sub.IsPoisoned = true;
            sub.UpdateLocalTime = this.time.Now.ToLocalTime();
            sub.PoisonEventCollectionVersion = poisonedEvent.EventCollectionVersion;
            sub.ExceptionMessage = this.serializer.Serialize(exception);
            sub.DeadLetterPayload = this.serializer.Serialize(poisonedEvent);
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            return this.Subscriptions
                        .Where(s => s.SubscriberStreamType == this.streamName && !s.IsPoisoned && !s.WasCanceled)
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
                var snapshotEntity = this.Snapshots.Where(s => s.StreamId == id && s.StreamType == this.streamName).SingleOrDefault();
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
                                .Where(e => e.StreamId == id && e.StreamType == this.streamName)
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
                var ex = new StreamNotFoundException(id, streamName);
                this.log.Error(ex, string.Format("Stream not found exception for stream {0} with id of {1}", streamName, id));
                throw ex;
            }

            return aggregate;
        }

        public void Save(T eventSourced, IEvent incomingEvent)
        {
            string key = null;

            try
            {
                // Check if incoming event is duplicate, if is not a cloaked event
                if (incomingEvent.EventId != Guid.Empty)
                {
                    // Check if incoming event is duplicate
                    if (this.Inbox.Any(e => e.EventId == incomingEvent.EventId))
                        // Incoming event is duplicate
                        return;
                }

                var now = this.time.Now;
                var localNow = this.time.Now.ToLocalTime();

                // No new events to persist
                if (eventSourced == null)
                {
                    if (!(incomingEvent is CloakedEvent))
                        // Log the incoming message in the inbox
                        this.Inbox.Add(this.inboxEntityFactory.Invoke(incomingEvent));

                    return;
                }

                if (eventSourced.Id == default(Guid))
                    throw new ArgumentOutOfRangeException("StreamId", $"The eventsourced of type {typeof(T).FullName} has a default GUID value for its stream id, which is not valid");

                var currentVersion = this.Events.Any(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamName)
                          ? this.Events
                            .Where(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamName)
                            .Max(e => e.Version)
                          : 0;

                var pendingEvents = eventSourced.PendingEvents;
                if (currentVersion + 1 != pendingEvents.First().Version)
                    throw new EventStoreConcurrencyException();

                // Log the incoming message in the inbox
                this.Inbox.Add(this.inboxEntityFactory.Invoke(incomingEvent));

                // Cache Memento And Publish Stream
                var snapshot = ((ISnapshotOriginator)eventSourced).SaveToSnapshot();

                // Cache in Sql Server
                var serializedMemento = this.serializer.Serialize(snapshot);

                var streamEntity = this.Snapshots.Where(s => s.StreamId == eventSourced.Id && s.StreamType == this.streamName).SingleOrDefault();
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
                        StreamType = this.streamName,
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
                    e.StreamType = this.streamName;
                    e.LocalTime = now;
                    e.UtcTime = localNow;

                    eventEntities.Add(
                        new EventEntity
                        {
                            StreamType = this.streamName,
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

                lock (this.dbLock)
                {
                    var eventCollectionBeforeCrash = this.eventCollectionVersion;
                    try
                    {
                        for (int i = 0; i < pendingEvents.Count; i++)
                        {
                            this.eventCollectionVersion += 1;
                            var @event = pendingEvents[i];
                            ((Message)@event).EventCollectionVersion = this.eventCollectionVersion;
                            var entity = eventEntities[i];
                            entity.EventCollectionVersion = this.eventCollectionVersion;
                            entity.Payload = this.serializer.Serialize(@event);

                            this.Events.Add(entity);
                        }

                        //var random = new Random();
                        //Thread.Sleep(random.Next(0, 1000));
                        this.CurrentEventCollectionVersion = this.eventCollectionVersion;
                    }
                    catch (Exception ex)
                    {
                        this.eventCollectionVersion = eventCollectionBeforeCrash;
                        throw ex;
                    }
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

        public bool IsDuplicate(Guid eventId, out Guid transactionId)
        {
            transactionId = default(Guid);
            var duplicate = this.Inbox.SingleOrDefault(e => e.EventId == eventId);
            if (duplicate == null)
                return false;

            transactionId = duplicate.TransactionId;
            return true;
        }

        public bool TryAddNewSubscriptionOnTheFly(string streamType, string url, string token)
        {
            if (this.Subscriptions.Any(s => s.SubscriberStreamType == this.streamName && s.StreamType == streamType))
                return false;

            this.Subscriptions.Add(new SubscriptionEntity
            {
                SubscriberStreamType = this.streamName,
                StreamType = streamType,
                Url = url,
                Token = token
            });
            return true;
        }

        public void PersistSubscriptionVersion(string subscription, long version)
        {
            var sub = this.Subscriptions.Where(s => s.StreamType == subscription && s.SubscriberStreamType == this.streamName).Single();
            sub.ProcessorBufferVersion = version;
            sub.UpdateLocalTime = DateTime.Now;
        }

        public long CurrentEventCollectionVersion { get; private set; }

        public string StreamName => this.streamName;
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
