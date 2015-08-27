using EventCentric.Database;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;

namespace EventCentric.EventSourcing
{
    public class EventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
        private static readonly string _streamType = typeof(T).Name;

        private readonly ITextSerializer serializer;
        private readonly ITimeProvider time;
        private readonly IGuidProvider guid;
        private readonly ObjectCache cache;

        private readonly Func<Guid, IMemento, T> originatorAggregateFactory;
        private readonly Func<IEventStoreDbContext> contextFactory;
        private readonly Action<T, IEventStoreDbContext> denormalizeIfApplicable;

        private readonly ISubscriptionInboxWriter subscriptionWriter;

        public EventStore(ITextSerializer serializer, Func<IEventStoreDbContext> contextFactory, ISubscriptionInboxWriter subscriptionWriter, ITimeProvider time, IGuidProvider guid)
        {
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(contextFactory, "contextFactory");
            Ensure.NotNull(time, "time");
            Ensure.NotNull(subscriptionWriter, "subscriptionWriter");
            Ensure.NotNull(guid, "guid");

            this.serializer = serializer;
            this.contextFactory = contextFactory;
            this.time = time;
            this.subscriptionWriter = subscriptionWriter;
            this.guid = guid;
            this.cache = new MemoryCache(_streamType);

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var mementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IMemento) });
            Ensure.CastIsValid(mementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");

            this.originatorAggregateFactory = (id, memento) => (T)mementoConstructor.Invoke(new object[] { id, memento });

            if (typeof(IDenormalizer).IsAssignableFrom(typeof(T)))
                this.denormalizeIfApplicable = (aggregate, context) => ((IDenormalizer)aggregate).Denormalize(context);
            else
                this.denormalizeIfApplicable = (aggregate, context) => { };
        }

        public T Find(Guid id)
        {
            // get memento from cache
            var cachedMemento = (Tuple<IMemento, DateTime?>)this.cache.Get(id.ToString());
            if (cachedMemento == null || !cachedMemento.Item2.HasValue)
            {
                // Return from SQL Server;
                using (var context = this.contextFactory.Invoke())
                {
                    var stream = context.Streams.Where(s => s.StreamId == id).SingleOrDefault();

                    if (stream != null)
                        cachedMemento = new Tuple<IMemento, DateTime?>(this.serializer.Deserialize<IMemento>(stream.Memento), null);
                    else
                        return null;
                }
            }

            return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
        }

        public T Get(Guid id)
        {
            // get memento from cache
            var cachedMemento = (Tuple<IMemento, DateTime?>)this.cache.Get(id.ToString());
            if (cachedMemento == null || !cachedMemento.Item2.HasValue)
            {
                // Return from SQL Server;
                using (var context = this.contextFactory.Invoke())
                {
                    var stream = context.Streams.Where(s => s.StreamId == id).SingleOrDefault();

                    if (stream != null)
                        cachedMemento = new Tuple<IMemento, DateTime?>(this.serializer.Deserialize<IMemento>(stream.Memento), null);
                    else
                        throw new StreamNotFoundException(id, _streamType);
                }
            }

            return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
        }

        public int Save(T eventSourced, IncomingEvent<IEvent> envelopedIncomingEvent)
        {
            var incomingEvent = envelopedIncomingEvent.Event;
            var pendingEvents = eventSourced.PendingEvents;
            if (pendingEvents.Length == 0)
                throw new ArgumentOutOfRangeException("pendingEvents");

            var key = eventSourced.Id.ToString();
            try
            {
                using (var context = this.contextFactory.Invoke())
                {
                    var versions = context.Events
                                          .Where(e => e.StreamId == eventSourced.Id)
                                          .AsCachedAnyEnumerable();

                    var currentVersion = 0;
                    if (versions.Any())
                        currentVersion = versions.Max(e => e.Version);

                    if (currentVersion + 1 != pendingEvents[0].Version)
                        throw new EventStoreConcurrencyException();

                    var now = this.time.Now;

                    foreach (var @event in pendingEvents)
                    {
                        ((Event)@event).EventId = this.guid.NewGuid;
                        ((Event)@event).StreamType = _streamType;

                        context.Events.Add(
                            new EventEntity
                            {
                                StreamId = @event.StreamId,
                                Version = @event.Version,
                                EventId = @event.EventId,
                                EventType = @event.GetType().Name,
                                CorrelationId = incomingEvent.EventId,
                                CreationDate = now,
                                Payload = this.serializer.Serialize(@event)
                            });
                    }

                    // Log incoming event in the subscription table
                    var message = new InboxEntity
                    {
                        EventId = incomingEvent.EventId,
                        StreamType = incomingEvent.StreamType,
                        StreamId = incomingEvent.StreamId,
                        Version = incomingEvent.Version,
                        EventType = incomingEvent.GetType().Name,
                        CreationDate = now,
                        Ignored = false,
                        Payload = this.serializer.Serialize(incomingEvent)
                    };
                    context.Inbox.Add(message);

                    throw new NotImplementedException("Need to implement subscription update");
                    //((DbContext)context).AddOrUpdate(
                    //    find: () => context
                    //                        .Subscriptions
                    //                        .Where(s => s.StreamId == correlatedEvent.StreamId && s.StreamType == correlatedEvent.StreamType)
                    //                        .SingleOrDefault(),
                    //    add: () => { throw new InvalidOperationException("Subscription does not exist!"); },
                    //    update: subscription =>
                    //    {
                    //        subscription.LastProcessedVersion = correlatedEvent.Version;
                    //        subscription.LastProcessedEventId = correlatedEvent.EventId;
                    //    });


                    // Cache Memento And Publish Stream
                    var memento = ((IMementoOriginator)eventSourced).SaveToMemento();

                    // Cache in Sql Server
                    var serializedMemento = this.serializer.Serialize(memento);

                    var streamEntity = ((DbContext)context).AddOrUpdate(
                        () => context.Streams.Where(s => s.StreamId == eventSourced.Id).SingleOrDefault(),
                        () => new StreamEntity
                        {
                            StreamId = eventSourced.Id,
                            Version = eventSourced.Version,
                            Memento = serializedMemento,
                            CreationDate = now
                        },
                        stream =>
                        {
                            stream.Version = eventSourced.Version;
                            stream.Memento = serializedMemento;
                            stream.CreationDate = now;
                        });

                    // Cache in memory
                    this.cache.Set(
                        key: key,
                        value: new Tuple<IMemento, DateTime?>(memento, now),
                        policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });

                    // Denormalize if applicable.
                    this.denormalizeIfApplicable(eventSourced, context);

                    context.SaveChanges();

                    //return streamEntity.StreamCollectionVersion;
                    throw new NotImplementedException();
                }
            }
            catch
            {
                // Mark cache as stale
                var item = (Tuple<IMemento, DateTime?>)this.cache.Get(key);
                if (item != null && item.Item2.HasValue)
                {
                    item = new Tuple<IMemento, DateTime?>(item.Item1, null);
                    this.cache.Set(
                        key,
                        item,
                        new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });
                }

                throw;
            }
        }
    }
}
