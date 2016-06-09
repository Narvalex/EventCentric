using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Persistence.SqlServer;
using EventCentric.Polling;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;

namespace EventCentric.Persistence
{
    // This event store does not denormalize
    public class OptimizedEventStore<T> : ISubscriptionRepository, IEventStore<T> where T : class, IEventSourced
    {
        private long eventCollectionVersion;
        private readonly string streamName;
        private readonly ILogger log;
        private readonly ITextSerializer serializer;
        private readonly IUtcTimeProvider time;
        private readonly IGuidProvider guid;
        private readonly ObjectCache cache;

        private readonly Func<Guid, IEnumerable<IEvent>, T> aggregateFactory;
        private readonly Func<Guid, ISnapshot, T> originatorAggregateFactory;
        private readonly Action<SqlConnection, SqlTransaction, DateTime, IEvent> addToInboxFactory;
        private readonly Func<string, string, bool> consumerFilter;

        private readonly string connectionString;
        private readonly SqlClientLite sql;
        private readonly object dbLock = new object();

        public OptimizedEventStore(string streamName, ITextSerializer serializer, string connectionString, IUtcTimeProvider time, IGuidProvider guid, ILogger log, bool persistIncomingPayloads, Func<string, string, bool> consumerFilter)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamName, nameof(streamName));
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(connectionString, nameof(connectionString));
            Ensure.NotNull(time, nameof(time));
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));

            this.streamName = streamName;
            this.serializer = serializer;
            this.connectionString = connectionString;
            this.time = time;
            this.guid = guid;
            this.log = log;
            this.consumerFilter = consumerFilter != null ? consumerFilter : EventStoreFuncs.DefaultFilter;
            this.cache = new MemoryCache(streamName);

            this.sql = new SqlClientLite(this.connectionString, timeoutInSeconds: 120);

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var fromMementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(ISnapshot) });
            Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
            this.originatorAggregateFactory = (id, memento) => (T)fromMementoConstructor.Invoke(new object[] { id, memento });

            var fromStreamConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
            Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
            this.aggregateFactory = (id, streamOfEvents) => (T)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });

            this.eventCollectionVersion = this.GetLatestEventCollectionVersionFromDb();
            this.CurrentEventCollectionVersion = this.eventCollectionVersion;

            if (persistIncomingPayloads)
                this.addToInboxFactory = this.AddToInboxWithPayload;
            else
                this.addToInboxFactory = this.AddToInboxWithoutPayload;

            // adding app subscription if missing
            var appSubCount = this.sql.ExecuteReaderFirstOrDefault(this.tryFindAppSubscription, r => r.GetInt32(0),
                                new SqlParameter("@SubscriberStreamType", this.streamName),
                                new SqlParameter("@StreamType", this.streamName + Constants.AppEventStreamNameSufix));

            if (appSubCount == 0)
            {
                var now = DateTime.Now;
                this.sql.ExecuteNonQuery(this.createAppSubscription,
                    new SqlParameter("@SubscriberStreamType", this.streamName),
                    new SqlParameter("@StreamType", this.streamName + Constants.AppEventStreamNameSufix),
                    new SqlParameter("@CreationLocalTime", now),
                    new SqlParameter("@UpdateLocalTime", now));
            }
        }

        private long GetLatestEventCollectionVersionFromDb()
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;

                    command.CommandText = @"select top 1 EventCollectionVErsion from EventStore.Events 
                                            where StreamType = @StreamType
                                            order by EventCollectionVersion desc";

                    command.Parameters.Add(new SqlParameter("@StreamType", this.streamName));

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return 0;

                        return (long)reader["EventCollectionVersion"];
                    }
                }
            }
        }

        public T Find(Guid id)
        {
            // get memento from cache
            var cachedMemento = (Tuple<ISnapshot, DateTime?>)this.cache.Get(id.ToString());
            if (cachedMemento == null || !cachedMemento.Item2.HasValue)
            {
                // try return memento from SQL Server;
                var snapshotEntity = this.sql
                                         .ExecuteReaderFirstOrDefault(this.findSnapshotQuery,
                                         r => new SnapshotEntity
                                         {
                                             Payload = r.GetString("Payload"),
                                         },
                                         new SqlParameter("@StreamType", this.streamName),
                                         new SqlParameter("@StreamId", id));

                if (snapshotEntity != null)
                    cachedMemento = new Tuple<ISnapshot, DateTime?>(this.serializer.Deserialize<ISnapshot>(snapshotEntity.Payload), null);
                else
                    return this.GetFromFullStreamOfEvents(id);
            }

            try
            {
                return this.originatorAggregateFactory.Invoke(id, cachedMemento.Item1);
            }
            catch (Exception ex)
            {
                this.log.Error(ex, $"An error ocurred while hydrating aggregate from snapshot. Now we will try to recover state from full stream of events for stream id {id.ToString()}");
                return this.GetFromFullStreamOfEvents(id);
            }
        }

        private T GetFromFullStreamOfEvents(Guid id)
        {
            var streamOfEvents =
                this.sql.ExecuteReader(this.getEventsQuery, r =>
                this.serializer.Deserialize<IEvent>(r.GetString("Payload")),
                new SqlParameter("@StreamType", this.streamName),
                new SqlParameter("@StreamId", id))
                .ToArray();

            if (streamOfEvents.Any())
                return this.aggregateFactory.Invoke(id, streamOfEvents);

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
                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    // Check if incoming event is duplicate, if is not a cloaked event
                    if (incomingEvent.EventId != Guid.Empty)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = this.isDuplicateQuery;
                            command.Parameters.Add(new SqlParameter("@EventId", incomingEvent.EventId));

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                    return;
                            }
                        }
                    }

                    using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            var now = this.time.Now;
                            var localNow = this.time.Now.ToLocalTime();

                            if (eventSourced == null)
                            {
                                if (!(incomingEvent is CloakedEvent))
                                    this.addToInboxFactory.Invoke(connection, transaction, localNow, incomingEvent);

                                transaction.Commit();
                                return;
                            }

                            if (eventSourced.Id == default(Guid))
                                throw new ArgumentOutOfRangeException("StreamId", $"The eventsourced of type {typeof(T).FullName} has a default GUID value for its stream id, which is not valid");

                            // get current version
                            long currentVersion = 0;
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandType = CommandType.Text;
                                command.Transaction = transaction;
                                command.CommandText = this.getStreamVersion;
                                command.Parameters.Add(new SqlParameter("@StreamType", this.streamName));
                                command.Parameters.Add(new SqlParameter("@StreamId", eventSourced.Id));

                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        currentVersion = reader.GetInt64("Version");
                                    }
                                }
                            }

                            var pendingEvents = eventSourced.PendingEvents;
                            if (currentVersion + 1 != pendingEvents.First().Version)
                                throw new EventStoreConcurrencyException();

                            // Log the incoming message in the inbox
                            this.addToInboxFactory.Invoke(connection, transaction, localNow, incomingEvent);

                            // Cache Memento And Publish Stream
                            var snapshot = ((ISnapshotOriginator)eventSourced).SaveToSnapshot();
                            var serializedMemento = this.serializer.Serialize(snapshot);

                            key = eventSourced.Id.ToString();
                            // Cache in memory
                            this.cache.Set(
                                key: key,
                                value: new Tuple<ISnapshot, DateTime?>(snapshot, now),
                                policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });

                            // Insert or Update snapshot
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = this.insertOrUpdateSnapshot;
                                command.Parameters.Add(new SqlParameter("@StreamType", this.streamName));
                                command.Parameters.Add(new SqlParameter("@StreamId", eventSourced.Id));
                                command.Parameters.Add(new SqlParameter("@Version", eventSourced.Version));
                                command.Parameters.Add(new SqlParameter("@Payload", serializedMemento));
                                command.Parameters.Add(new SqlParameter("@CreationLocalTime", localNow));
                                command.Parameters.Add(new SqlParameter("@UpdateLocalTime", localNow));

                                command.ExecuteNonQuery();
                            }

                            var eventEntities = new EventEntity[pendingEvents.Count];
                            for (int i = 0; i < pendingEvents.Count; i++)
                            {
                                var @event = pendingEvents[i];
                                var e = (Message)@event;
                                e.TransactionId = incomingEvent.TransactionId;
                                e.EventId = this.guid.NewGuid();
                                e.StreamType = this.streamName;

                                eventEntities[i] = new EventEntity
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
                                };
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

                                        // Insert each event
                                        using (var command = connection.CreateCommand())
                                        {
                                            command.Transaction = transaction;
                                            command.CommandType = CommandType.Text;
                                            command.CommandText = this.appendEventCommand;
                                            command.Parameters.Add(new SqlParameter("@StreamType", entity.StreamType));
                                            command.Parameters.Add(new SqlParameter("@StreamId", entity.StreamId));
                                            command.Parameters.Add(new SqlParameter("@Version", entity.Version));
                                            command.Parameters.Add(new SqlParameter("@TransactionId", entity.TransactionId));
                                            command.Parameters.Add(new SqlParameter("@EventId", entity.EventId));
                                            command.Parameters.Add(new SqlParameter("@EventType", entity.EventType));
                                            command.Parameters.Add(new SqlParameter("@CorrelationId", entity.CorrelationId));
                                            command.Parameters.Add(new SqlParameter("@EventCollectionVersion", entity.EventCollectionVersion));
                                            command.Parameters.Add(new SqlParameter("@LocalTime", entity.LocalTime));
                                            command.Parameters.Add(new SqlParameter("@UtcTime", entity.UtcTime));
                                            command.Parameters.Add(new SqlParameter("@Payload", entity.Payload));

                                            command.ExecuteNonQuery();
                                        }
                                    }
                                    transaction.Commit();

                                    this.CurrentEventCollectionVersion = this.eventCollectionVersion;
                                }
                                catch (Exception ex)
                                {
                                    this.eventCollectionVersion = eventCollectionBeforeCrash;
                                    throw ex;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error(ex, $"An error ocurred while storing events while processing incoming event of type '{incomingEvent.GetType().Name}'");

                // Mark cache as stale
                if (key != null)
                {
                    var item = (Tuple<ISnapshot, DateTime?>)this.cache.Get(key);
                    if (item != null && item.Item2.HasValue)
                    {
                        item = new Tuple<ISnapshot, DateTime?>(item.Item1, null);
                        this.cache.Set(
                            key,
                            item,
                            new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });
                    }
                }

                throw;
            }
        }

        private void AddToInboxWithPayload(SqlConnection connection, SqlTransaction transaction, DateTime localNow, IEvent incomingEvent)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandType = CommandType.Text;
                command.CommandText = this.addToInboxWithPayloadCommand;
                command.Parameters.Add(new SqlParameter("@InboxStreamType", this.streamName));
                command.Parameters.Add(new SqlParameter("@EventId", incomingEvent.EventId));
                command.Parameters.Add(new SqlParameter("@TransactionId", incomingEvent.TransactionId));
                command.Parameters.Add(new SqlParameter("@StreamType", incomingEvent.StreamType));
                command.Parameters.Add(new SqlParameter("@StreamId", incomingEvent.StreamId));
                command.Parameters.Add(new SqlParameter("@Version", incomingEvent.Version));
                command.Parameters.Add(new SqlParameter("@EventType", incomingEvent.GetType().Name));
                command.Parameters.Add(new SqlParameter("@EventCollectionVersion", incomingEvent.EventCollectionVersion));
                command.Parameters.Add(new SqlParameter("@CreationLocalTime", localNow));
                command.Parameters.Add(new SqlParameter("@Payload", this.serializer.Serialize(incomingEvent)));

                command.ExecuteNonQuery();
            }
        }

        private void AddToInboxWithoutPayload(SqlConnection connection, SqlTransaction transaction, DateTime localNow, IEvent incomingEvent)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandType = CommandType.Text;
                command.CommandText = this.addToInboxWithoutPayloadCommand;
                command.Parameters.Add(new SqlParameter("@InboxStreamType", this.streamName));
                command.Parameters.Add(new SqlParameter("@EventId", incomingEvent.EventId));
                command.Parameters.Add(new SqlParameter("@CreationLocalTime", localNow));
                command.Parameters.Add(new SqlParameter("@TransactionId", incomingEvent.TransactionId));

                command.ExecuteNonQuery();
            }
        }

        public bool IsDuplicate(Guid eventId, out Guid transactionId)
        {
            transactionId = Guid.Empty;
            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = this.isDuplicateQuery;
                    command.Parameters.Add(new SqlParameter("@EventId", eventId));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            transactionId = reader.GetGuid("TransactionId");
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        public void DeleteSnapshot(Guid streamId)
        {
            cache.Remove(streamId.ToString());
            this.sql.ExecuteNonQuery(this.removeSnapshotCommand,
                new SqlParameter("@StreamId", streamId), new SqlParameter(@"StreamType", this.streamName));
        }

        public long CurrentEventCollectionVersion { get; private set; }

        public string StreamName => this.streamName;

        /// <summary>
        /// FindEvents
        /// </summary>
        /// <returns>Events if found, otherwise return empty list.</returns>
        public SerializedEvent[] FindEventsForConsumer(long from, long to, int quantity, string consumer)
        {
            return this.sql.ExecuteReader(this.findEventsQuery,
                r => new SerializedEvent(r.GetInt64("EventCollectionVersion"), r.GetString("Payload")),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@StreamType", this.streamName),
                new SqlParameter("@LastReceivedVersion", from),
                new SqlParameter("@MaxVersion", to))
                .Where(e => this.consumerFilter(consumer, e.Payload))
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
            var events = this.sql.ExecuteReader(this.findEventsWithStreamIdQuery,
                r => new SerializedEvent(r.GetInt64("EventCollectionVersion"), r.GetString("Payload")),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@StreamType", this.streamName),
                new SqlParameter("@LastReceivedVersion", from),
                new SqlParameter("@MaxVersion", to),
                new SqlParameter("@StreamId", streamId))
                .Where(e => this.consumerFilter(consumer, e.Payload))
                .Select(e =>
                            EventStoreFuncs.ApplyConsumerFilter(
                                new SerializedEvent(e.EventCollectionVersion, e.Payload),
                                consumer,
                                this.serializer,
                                this.consumerFilter))
                .ToArray();

            return events.Length > 0
                ? events
                : new SerializedEvent[] { new SerializedEvent(to, this.serializer.Serialize(CloakedEvent.New(Guid.Empty, to, this.streamName))) };
        }

        public SubscriptionBuffer[] GetSubscriptions()
        {
            return this.sql.ExecuteReader(this.getSubscriptionsQuery,
                r => new SubscriptionBuffer(
                        r.GetString("StreamType"),
                        r.GetString("Url"),
                        r.SafeGetString("Token"),
                        r.GetInt64("ProcessorBufferVersion") - 1, // We substract one version in order to set the current version bellow the last one, in case that first event was not yet processed.
                        false),
                        new SqlParameter("@SubscriberStreamType", this.streamName))
                        .ToArray();
        }

        public void FlagSubscriptionAsPoisoned(IEvent poisonedEvent, PoisonMessageException exception)
        {
            using (var context = new EventStoreDbContext(false, this.connectionString))
            {
                var subscription = context.Subscriptions.Where(s => s.StreamType == poisonedEvent.StreamType && s.SubscriberStreamType == this.streamName).Single();
                subscription.IsPoisoned = true;
                subscription.UpdateLocalTime = this.time.Now.ToLocalTime();
                subscription.PoisonEventCollectionVersion = poisonedEvent.EventCollectionVersion;
                try
                {
                    subscription.ExceptionMessage = this.serializer.Serialize(exception);
                }
                catch (Exception)
                {
                    subscription.ExceptionMessage = string.Format("Exception type: {0}. Exception message: {1}. Inner exception: {2}", exception.GetType().Name, exception.Message, exception.InnerException.Message != null ? exception.InnerException.Message : "null");
                }
                try
                {
                    subscription.DeadLetterPayload = this.serializer.Serialize(poisonedEvent);
                }
                catch (Exception)
                {
                    subscription.DeadLetterPayload = string.Format("EventType: {0}", poisonedEvent.GetType().Name);
                }

                context.SaveChanges();
            }
        }

        public bool TryAddNewSubscriptionOnTheFly(string streamType, string url, string token)
        {
            using (var context = new EventStoreDbContext(false, this.connectionString))
            {
                if (context.Subscriptions.Any(s => s.SubscriberStreamType == this.streamName && s.StreamType == streamType))
                    return false;

                var now = DateTime.Now;
                context.Subscriptions.Add(new SubscriptionEntity
                {
                    SubscriberStreamType = this.streamName,
                    StreamType = streamType,
                    Url = url,
                    Token = token,
                    CreationLocalTime = now,
                    UpdateLocalTime = now
                });

                context.SaveChanges();
                return true;
            }
        }

        public void PersistSubscriptionVersion(string subscription, long version)
        {
            this.sql.ExecuteNonQuery(this.updateSubscriptionScript,
                new SqlParameter("@SubscriberStreamType", this.streamName),
                new SqlParameter("@StreamType", subscription),
                new SqlParameter("@Version", version),
                new SqlParameter("@UpdateLocalTime", DateTime.Now));
        }


        #region Scripts
        private readonly string findSnapshotQuery =
        @"select Payload from EventStore.Snapshots
where StreamType = @StreamType 
and StreamId = @StreamId";

        private readonly string getEventsQuery =
        @"select Payload from EventStore.Events 
where StreamType = @StreamType
and StreamId = @StreamId
order by [Version]";

        private readonly string isDuplicateQuery =
        "select InboxId, TransactionId from eventstore.inbox where EventId = @EventId";

        private readonly string tryFindAppSubscription =
        @"select count(*) from EventStore.Subscriptions
where SubscriberStreamType = @SubscriberStreamType
and StreamType = @StreamType";

        private readonly string removeSnapshotCommand =
        @"delete from EventStore.Snapshots
where StreamType = @StreamType 
and StreamId = @StreamId";

        private readonly string getStreamVersion =
        @"select top 1 [Version] from EventStore.Events 
where StreamType = @StreamType
and StreamId = @StreamId
order by [Version] desc";

        private readonly string addToInboxWithPayloadCommand =
        @"INSERT INTO [EventStore].[Inbox]
    ([InboxStreamType]
    ,[EventId]
    ,[TransactionId]
    ,[StreamType]
    ,[StreamId]
    ,[Version]
    ,[EventType]
    ,[EventCollectionVersion]
    ,[CreationLocalTime]
    ,[Payload])
VALUES
    (@InboxStreamType, 
    @EventId, 
    @TransactionId, 
    @StreamType, 
    @StreamId, 
    @Version, 
    @EventType, 
    @EventCollectionVersion, 
    @CreationLocalTime,
    @Payload)";

        private readonly string addToInboxWithoutPayloadCommand =
        @"INSERT INTO [EventStore].[Inbox]
    ([InboxStreamType]
    ,[EventId]
    ,[CreationLocalTime]
    ,[TransactionId])
VALUES
    (@InboxStreamType, 
    @EventId,
    @CreationLocalTime,
    @TransactionId)";

        private readonly string insertOrUpdateSnapshot =
        @"if exists (select StreamType from EventStore.Snapshots where StreamId = @StreamId AND StreamType = @StreamType)
BEGIN
UPDATE [EventStore].[Snapshots]
   SET [Version] = @Version
      ,[Payload] = @Payload
      ,[UpdateLocalTime] = @UpdateLocalTime
 WHERE StreamId = @StreamId AND StreamType = @StreamType
END
ELSE
BEGIN
INSERT INTO [EventStore].[Snapshots]
           ([StreamType]
           ,[StreamId]
           ,[Version]
           ,[Payload]
           ,[CreationLocalTime]
           ,[UpdateLocalTime])
     VALUES
           (@StreamType
           ,@StreamId
           ,@Version
           ,@Payload
           ,@CreationLocalTime
           ,@UpdateLocalTime)
END";

        private readonly string createAppSubscription =
        @"INSERT INTO [EventStore].[Subscriptions]
    ([SubscriberStreamType]
    ,[StreamType]
    ,[Url]
    ,[Token]
    ,[ProcessorBufferVersion]
    ,[IsPoisoned]
    ,[WasCanceled]
    ,[PoisonEventCollectionVersion]
    ,[DeadLetterPayload]
    ,[ExceptionMessage]
    ,[CreationLocalTime]
    ,[UpdateLocalTime])
VALUES
    (@SubscriberStreamType
    ,@StreamType
    ,'none'
    ,'#token'
    ,0
    ,0
    ,1
    ,null
    ,null
    ,null
    ,@CreationLocalTime
    ,@UpdateLocalTime)";

        private readonly string appendEventCommand =
        @"INSERT INTO [EventStore].[Events]
    ([StreamType]
    ,[StreamId]
    ,[Version]
    ,[TransactionId]
    ,[EventId]
    ,[EventType]
    ,[CorrelationId]
    ,[EventCollectionVersion]
    ,[LocalTime]
    ,[UtcTime]
    ,[Payload])
VALUES
    (@StreamType
    ,@StreamId
    ,@Version
    ,@TransactionId
    ,@EventId
    ,@EventType
    ,@CorrelationId
    ,@EventCollectionVersion
    ,@LocalTime
    ,@UtcTime
    ,@Payload)";

        private readonly string findEventsQuery =
        @"select top (@Quantity)
EventCollectionVersion,
Payload
from EventStore.Events
where StreamType = @StreamType
and EventCollectionVersion > @LastReceivedVersion
and EventCollectionVersion <= @MaxVersion
order by EventCollectionVersion";

        private readonly string findEventsWithStreamIdQuery =
            @"select top (@Quantity)
EventCollectionVersion,
Payload
from EventStore.Events
where StreamType = @StreamType
and EventCollectionVersion > @LastReceivedVersion
and EventCollectionVersion <= @MaxVersion
and StreamId = @StreamId
order by EventCollectionVersion";

        private readonly string getSubscriptionsQuery =
@"SELECT [StreamType]
      ,[Url]
      ,[Token]
      ,[ProcessorBufferVersion]
  FROM [EventStore].[Subscriptions]
  WHERE SubscriberStreamType = @SubscriberStreamType
  AND IsPoisoned = 0
  AND WasCanceled = 0";

        private readonly string updateSubscriptionScript =
@"UPDATE [EventStore].[Subscriptions]
   SET [ProcessorBufferVersion] = @Version
      ,[UpdateLocalTime] = @UpdateLocalTime
 WHERE SubscriberStreamType = @SubscriberStreamType
 AND StreamType = @StreamType";
        #endregion
    }
}
