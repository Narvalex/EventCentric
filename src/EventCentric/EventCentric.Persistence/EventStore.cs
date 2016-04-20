using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace EventCentric.Persistence
{
    public class EventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
        private long eventCollectionVersion = 0;
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

            using (var context = this.contextFactory.Invoke(true))
            {
                if (context.Events.Any(e => e.StreamType == this.streamType))
                    this.eventCollectionVersion = context.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);
            }
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
                    var snapshotEntity = context.Snapshots.Where(s => s.StreamId == id && s.StreamType == this.streamType).SingleOrDefault();

                    if (snapshotEntity != null)
                        cachedMemento = new Tuple<ISnapshot, DateTime?>(this.serializer.Deserialize<ISnapshot>(snapshotEntity.Payload), null);
                    else
                        return this.GetFromFullStreamOfEvents(id, context);
                }
            }

            try
            {
                return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
            }
            catch (Exception ex)
            {
                this.log.Error(ex, $"An error ocurred while hydrating aggregate from snapshot. Now we will try to recover state from full stream of events for stream id {id.ToString()}");
                using (var context = this.contextFactory.Invoke(true))
                {
                    return this.GetFromFullStreamOfEvents(id, context);
                }
            }
        }

        private T GetFromFullStreamOfEvents(Guid id, IEventStoreDbContext context)
        {
            // if memento not found then try get full stream
            var streamOfEvents = context.Events
                           .Where(e => e.StreamId == id && e.StreamType == this.streamType)
                           .OrderBy(e => e.Version)
                           .AsEnumerable()
                           .Select(e => this.serializer.Deserialize<IEvent>(e.Payload))
                           .AsCachedAnyEnumerable();

            if (streamOfEvents.Any())
                return this.aggregateFactory.Invoke(id, streamOfEvents);

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
                using (var context = this.contextFactory.Invoke(false))
                {
                    // Check if incoming event is duplicate
                    if (this.IsDuplicate(incomingEvent.EventId, context))
                        // Incoming event is duplicate
                        return this.eventCollectionVersion;

                    var currentVersion = context.Events.Any(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamType)
                                          ? context.Events
                                            .Where(e => e.StreamId == eventSourced.Id && e.StreamType == this.streamType)
                                            .Max(e => e.Version)
                                          : 0;

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
                        CreationLocalTime = localNow,
                        Ignored = false,
                        Payload = this.serializer.Serialize(incomingEvent)
                    };
                    context.Inbox.Add(message);

                    // Update subscription
                    try
                    {
                        var subscription = context.Subscriptions.Where(s => s.StreamType == incomingEvent.StreamType && s.SubscriberStreamType == this.streamType).Single();
                        subscription.ProcessorBufferVersion = incomingEvent.ProcessorBufferVersion;
                        subscription.UpdateLocalTime = localNow;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"The incoming event belongs to a an stream type of {incomingEvent.StreamType}, but the event store could not found a subscription for that stream type.", ex);
                    }


                    // Cache Memento And Publish Stream
                    var snapshot = ((ISnapshotOriginator)eventSourced).SaveToSnapshot();

                    // Cache in Sql Server
                    var serializedMemento = this.serializer.Serialize(snapshot);

                    var streamEntity = context.Snapshots.Where(s => s.StreamId == eventSourced.Id && s.StreamType == this.streamType).SingleOrDefault();
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
                        context.Snapshots.Add(streamEntity);
                    }

                    // Cache in memory
                    this.cache.Set(
                        key: key,
                        value: new Tuple<ISnapshot, DateTime?>(snapshot, now),
                        policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });

                    // Denormalize if applicable.
                    this.denormalizeIfApplicable(eventSourced, context);


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

                    long eventCollectionVersionToPublish;
                    lock (this)
                    {
                        var eventCollectionBeforeCrash = this.eventCollectionVersion;
                        try
                        {
                            for (int i = 0; i < pendingEvents.Count; i++)
                            {
                                var ecv = Interlocked.Increment(ref this.eventCollectionVersion);
                                var @event = pendingEvents[i];
                                ((Message)@event).EventCollectionVersion = ecv;
                                var entity = eventEntities[i];
                                entity.EventCollectionVersion = ecv;
                                entity.Payload = this.serializer.Serialize(@event);
                                context.Events.Add(entity);
                            }
                            context.SaveChanges();

                            eventCollectionVersionToPublish = pendingEvents.Last().EventCollectionVersion;
                            //return context.Events.Where(e => e.StreamType == this.streamType).Max(e => e.EventCollectionVersion);

                        }
                        catch (Exception ex)
                        {
                            this.eventCollectionVersion = eventCollectionBeforeCrash;
                            throw ex;
                        }
                    }

                    return eventCollectionVersionToPublish;
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

        public void DeleteSnapshot(Guid streamId)
        {
            cache.Remove(streamId.ToString());
            using (var context = this.contextFactory.Invoke(false))
            {
                context.Snapshots.Remove(context.Snapshots.Single(x => x.StreamId == streamId));
                context.SaveChanges();
            }
        }
    }
}
