using EventCentric.Database;
using EventCentric.Log;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;

namespace EventCentric.EventSourcing
{
    public class EventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
        private readonly string streamType;
        private readonly ILogger log;
        private readonly ITextSerializer serializer;
        private readonly IUtcTimeProvider time;
        private readonly IGuidProvider guid;
        private readonly ObjectCache cache;

        private readonly Func<Guid, IEnumerable<IEvent>, T> aggregateFactory;
        private readonly Func<Guid, ISnapshot, T> originatorAggregateFactory;

        private readonly Func<bool, IEventStoreDbContext> contextFactory;
        private readonly Action<T, IEventStoreDbContext> denormalizeIfApplicable;

        public EventStore(string streamType, ITextSerializer serializer, Func<bool, IEventStoreDbContext> contextFactory, IUtcTimeProvider time, IGuidProvider guid, ILogger log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNull(contextFactory, nameof(contextFactory));
            Ensure.NotNull(time, nameof(time));
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));

            this.streamType = streamType;
            this.serializer = serializer;
            this.contextFactory = contextFactory;
            this.time = time;
            this.guid = guid;
            this.log = log;
            this.cache = new MemoryCache(streamType);

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var fromMementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(ISnapshot) });
            Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
            this.originatorAggregateFactory = (id, memento) => (T)fromMementoConstructor.Invoke(new object[] { id, memento });

            var fromStreamConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
            Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
            this.aggregateFactory = (id, streamOfEvents) => (T)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });

            if (typeof(IDenormalizer).IsAssignableFrom(typeof(T)))
                this.denormalizeIfApplicable = (aggregate, context) => ((IDenormalizer)aggregate).UpdateReadModel(context);
            else
                this.denormalizeIfApplicable = (aggregate, context) => { };
        }

        public T Find(Guid id)
        {
            // get memento from cache
            var cachedMemento = (Tuple<ISnapshot, DateTime?>)this.cache.Get(id.ToString());
            if (cachedMemento == null || !cachedMemento.Item2.HasValue)
            {
                // try return memento from SQL Server;
                using (var context = this.contextFactory.Invoke(true))
                {
                    var snapshotEntity = context.Snapshots.Where(s => s.StreamId == id).SingleOrDefault();

                    if (snapshotEntity != null)
                        cachedMemento = new Tuple<ISnapshot, DateTime?>(this.serializer.Deserialize<ISnapshot>(snapshotEntity.Payload), null);
                    else
                    {
                        // if memento not found then try get full stream
                        var streamOfEvents = context.Events
                                       .Where(e => e.StreamId == id)
                                       .OrderBy(e => e.Version)
                                       .AsEnumerable()
                                       .Select(e => this.serializer.Deserialize<IEvent>(e.Payload))
                                       .AsCachedAnyEnumerable();

                        if (streamOfEvents.Any())
                            return aggregateFactory.Invoke(id, streamOfEvents);

                        return null;
                    }
                }
            }

            return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
        }

        public T Get(Guid id)
        {
            var aggregate = this.Find(id);
            if (aggregate == null)
            {
                var ex = new StreamNotFoundException(id, streamType);
                this.log.Error(ex, "Stream not found exception for stream {0} with id of {1}", streamType, id);
                throw ex;
            }

            return aggregate;
        }

        public long Save(T eventSourced, IEvent incomingEvent)
        {
            var pendingEvents = eventSourced.PendingEvents;
            if (pendingEvents.Length == 0)
                throw new ArgumentOutOfRangeException("pendingEvents");

            var key = eventSourced.Id.ToString();
            try
            {
                using (var context = this.contextFactory.Invoke(false))
                {
                    var versions = context.Events
                                          .Where(e => e.StreamId == eventSourced.Id)
                                          .AsCachedAnyEnumerable();

                    long currentVersion = 0;
                    if (versions.Any())
                        currentVersion = versions.Max(e => e.Version);

                    // Check if incoming event is duplicate
                    if (this.IsDuplicate(incomingEvent.EventId, context))
                        // Incoming event is duplicate
                        return currentVersion;

                    if (currentVersion + 1 != pendingEvents[0].Version)
                        throw new EventStoreConcurrencyException();

                    var now = this.time.Now;
                    var localNow = this.time.Now.ToLocalTime();

                    foreach (var @event in pendingEvents)
                    {
                        @event.AsStoredEvent(incomingEvent.TransactionId, this.guid.NewGuid(), streamType, now, localNow);

                        context.Events.Add(
                            new EventEntity
                            {
                                StreamId = @event.StreamId,
                                Version = @event.Version,
                                EventId = @event.EventId,
                                TransactionId = @event.TransactionId,
                                EventType = @event.GetType().Name,
                                CorrelationId = incomingEvent.EventId,
                                LocalTime = localNow,
                                UtcTime = now,
                                Payload = this.serializer.Serialize(@event)
                            });
                    }

                    // Log the incoming message in the inbox
                    var message = new InboxEntity
                    {
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
                    context.Inbox.Add(message);

                    // Update subscription
                    try
                    {
                        var subscription = context.Subscriptions.Where(s => s.StreamType == incomingEvent.StreamType).Single();
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

                    var streamEntity = ((DbContext)context).AddOrUpdate(
                        () => context.Snapshots.Where(s => s.StreamId == eventSourced.Id).SingleOrDefault(),
                        () => new SnapshotEntity
                        {
                            StreamId = eventSourced.Id,
                            Version = eventSourced.Version,
                            Payload = serializedMemento,
                            CreationLocalTime = localNow,
                            UpdateLocalTime = localNow
                        },
                        snapshotEntity =>
                        {
                            snapshotEntity.Version = eventSourced.Version;
                            snapshotEntity.Payload = serializedMemento;
                            snapshotEntity.UpdateLocalTime = localNow;
                        });

                    // Cache in memory
                    this.cache.Set(
                        key: key,
                        value: new Tuple<ISnapshot, DateTime?>(snapshot, now),
                        policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });

                    // Denormalize if applicable.
                    this.denormalizeIfApplicable(eventSourced, context);

                    context.SaveChanges();

                    return context.Events.Max(e => e.EventCollectionVersion);
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

        public bool IsDuplicate(Guid eventId)
        {
            using (var context = this.contextFactory.Invoke(true))
            {
                return this.IsDuplicate(eventId, context);
            }
        }

        private bool IsDuplicate(Guid eventId, IEventStoreDbContext context)
            => context.Inbox.Any(e => e.EventId == eventId);
    }
}
