using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Serialization;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace EventCentric.Persistence
{
    // This event store does not denormalize
    public class OptimizedEventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
        private long eventCollectionVersion;
        private readonly string streamType;
        private readonly ILogger log;
        private readonly ITextSerializer serializer;
        private readonly IUtcTimeProvider time;
        private readonly IGuidProvider guid;
        private readonly ObjectCache cache;

        private readonly Func<Guid, IEnumerable<IEvent>, T> aggregateFactory;
        private readonly Func<Guid, ISnapshot, T> originatorAggregateFactory;

        private readonly string connectionString;
        private readonly SqlClientLite sql;

        public OptimizedEventStore(string streamType, ITextSerializer serializer, string connectionString, IUtcTimeProvider time, IGuidProvider guid, ILogger log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));
            Ensure.NotNull(serializer, nameof(serializer));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(connectionString, nameof(connectionString));
            Ensure.NotNull(time, nameof(time));
            Ensure.NotNull(guid, nameof(guid));
            Ensure.NotNull(log, nameof(log));

            this.streamType = streamType;
            this.serializer = serializer;
            this.connectionString = connectionString;
            this.time = time;
            this.guid = guid;
            this.log = log;
            this.cache = new MemoryCache(streamType);

            this.sql = new SqlClientLite(this.connectionString, timeoutInSeconds: 120);

            /// TODO: could be replaced with a compiled lambda to make it more performant.
            var fromMementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(ISnapshot) });
            Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
            this.originatorAggregateFactory = (id, memento) => (T)fromMementoConstructor.Invoke(new object[] { id, memento });

            var fromStreamConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
            Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
            this.aggregateFactory = (id, streamOfEvents) => (T)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });

            this.eventCollectionVersion = this.GetLatestEventCollectionVersionFromDb();
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

                    command.Parameters.Add(new SqlParameter("@StreamType", this.streamType));

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
                                         new SqlParameter("@StreamType", this.streamType),
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
                new SqlParameter("@StreamType", this.streamType),
                new SqlParameter("@StreamId", id));

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
                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    // Check if incoming event is duplicate
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = this.isDuplicateQuery;
                        command.Parameters.Add(new SqlParameter("@EventId", incomingEvent.EventId));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                                return this.eventCollectionVersion;
                        }
                    }

                    // get current version
                    long currentVersion = 0;
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = this.getStreamVersion;
                        command.Parameters.Add(new SqlParameter("@StreamType", this.streamType));
                        command.Parameters.Add(new SqlParameter("@StreamId", eventSourced.Id));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentVersion = reader.GetInt64("Version");
                            }
                        }
                    }

                    if (currentVersion + 1 != pendingEvents.First().Version)
                        throw new EventStoreConcurrencyException();

                    var now = this.time.Now;
                    var localNow = this.time.Now.ToLocalTime();

                    // Cache Memento And Publish Stream
                    var snapshot = ((ISnapshotOriginator)eventSourced).SaveToSnapshot();

                    // Cache in Sql Server
                    var serializedMemento = this.serializer.Serialize(snapshot);


                    // Cache in memory
                    this.cache.Set(
                        key: key,
                        value: new Tuple<ISnapshot, DateTime?>(snapshot, now),
                        policy: new CacheItemPolicy { AbsoluteExpiration = this.time.OffSetNow.AddMinutes(30) });

                    using (var transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            // Log the incoming message in the inbox
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = this.addToInboxCommand;
                                command.Parameters.Add(new SqlParameter("@InboxStreamType", this.streamType));
                                command.Parameters.Add(new SqlParameter("@EventId", incomingEvent.EventId));
                                command.Parameters.Add(new SqlParameter("@TransactionId", incomingEvent.TransactionId));
                                command.Parameters.Add(new SqlParameter("@StreamType", incomingEvent.StreamType));
                                command.Parameters.Add(new SqlParameter("@StreamId", incomingEvent.StreamId));
                                command.Parameters.Add(new SqlParameter("@Version", incomingEvent.Version));
                                command.Parameters.Add(new SqlParameter("@EventType", incomingEvent.GetType().Name));
                                command.Parameters.Add(new SqlParameter("@EventCollectionVersion", incomingEvent.EventCollectionVersion));
                                command.Parameters.Add(new SqlParameter("@CreationLocalTime", localNow));
                                command.Parameters.Add(new SqlParameter("@Ignored", false));
                                command.Parameters.Add(new SqlParameter("@Payload", this.serializer.Serialize(incomingEvent)));

                                command.ExecuteNonQuery();
                            }

                            // Update subscription
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = this.updateSubscriptionCommand;
                                command.Parameters.Add(new SqlParameter("@ProcessorBufferVersion", incomingEvent.ProcessorBufferVersion));
                                command.Parameters.Add(new SqlParameter("@UpdateLocalTime", localNow));
                                command.Parameters.Add(new SqlParameter("@StreamType", incomingEvent.StreamType));
                                command.Parameters.Add(new SqlParameter("@SubscriberStreamType", this.streamType));

                                command.ExecuteNonQuery();
                            }

                            // Insert or Update snapshot
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandType = CommandType.Text;
                                command.CommandText = this.insertOrUpdateSnapshot;
                                command.Parameters.Add(new SqlParameter("@StreamType", this.streamType));
                                command.Parameters.Add(new SqlParameter("@StreamId", eventSourced.Id));
                                command.Parameters.Add(new SqlParameter("@Version", eventSourced.Version));
                                command.Parameters.Add(new SqlParameter("@Payload", serializedMemento));
                                command.Parameters.Add(new SqlParameter("@CreationLocalTime", localNow));
                                command.Parameters.Add(new SqlParameter("@UpdateLocalTime", localNow));

                                command.ExecuteNonQuery();
                            }

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
                        return reader.Read();
                    }
                }
            }
        }

        public void DeleteSnapshot(Guid streamId)
        {
            cache.Remove(streamId.ToString());
            this.sql.ExecuteNonQuery(this.removeSnapshotCommand,
                new SqlParameter("@StreamId", streamId), new SqlParameter(@"StreamType", this.streamType));
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
"select InboxId from eventstore.inbox where EventId = @EventId";

        private readonly string removeSnapshotCommand =
@"delete from EventStore.Snapshots
where StreamType = @StreamType 
and StreamId = @StreamId";

        private readonly string getStreamVersion =
@"select top 1 [Version] from EventStore.Events 
where StreamType = @StreamType
and StreamId = @StreamId
order by [Version] desc";

        private readonly string addToInboxCommand =
@"INSERT INTO [EventStore].[Inbox]
    ([InboxStreamType]
    ,[EventId]
    ,[TransactionId]
    ,[StreamType]
    ,[StreamId]
    ,[Version]
    ,[EventType]
    ,[EventCollectionVersion]
    ,[Ignored]
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
    @Ignored, 
    @CreationLocalTime,
    @Payload)";

        private readonly string updateSubscriptionCommand =
@"UPDATE [EventStore].[Subscriptions]
SET [ProcessorBufferVersion] = @ProcessorBufferVersion,
    [UpdateLocalTime] = @UpdateLocalTime
WHERE [StreamType] = @StreamType AND [SubscriberStreamType] = @SubscriberStreamType";

        private readonly string insertOrUpdateSnapshot =
@"if exists (select * from EventStore.Snapshots where StreamId = @StreamId AND StreamType = @StreamType)
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
        #endregion
    }
}
