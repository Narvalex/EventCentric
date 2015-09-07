using EventCentric.Config;
using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.NodeFactory.Log;
using EventCentric.Publishing;
using EventCentric.Queueing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public class ClientNodeFactory<T>
    {
        public static INode CreateNode(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var eventStoreConfig = EventStoreConfig.GetConfig();
            var connectionString = eventStoreConfig.ConnectionString;

            Func<bool, EventQueueDbContext> eventQueueDbContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var log = Logger.ResolvedLogger;

            var node = new ClientNode(bus, log);

            var queueWriter = new QueueWriter<T>(eventQueueDbContextFactory, serializer, time, guid);
            var eventDao = new EventDao(eventQueueDbContextFactory);
            var eventBus = new EventQueue(bus, log, queueWriter);
            var eventPublisher = new Publisher<T>(bus, log, eventDao, eventStoreConfig.PushMaxCount, eventStoreConfig.PollAttemptsMaxCount);

            var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 2, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);

            return node;
        }

        public static INode CreateCrudNode<TDbContext>(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true) where TDbContext : IEventQueueDbContext
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var eventStoreConfig = EventStoreConfig.GetConfig();
            var connectionString = eventStoreConfig.ConnectionString;

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventQueueDbContext> eventQueueDbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var log = Logger.ResolvedLogger;

            var node = new ClientNode(bus, log);

            var queueWriter = new CrudQueueWriter<T>(eventQueueDbContextFactory, serializer, time, guid);
            var eventDao = new EventDao(eventQueueDbContextFactory);
            var eventBus = new CrudEventQueue(bus, log, queueWriter);
            var eventPublisher = new Publisher<T>(bus, log, eventDao, eventStoreConfig.PushMaxCount, eventStoreConfig.PollAttemptsMaxCount);

            var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 2, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);

            return node;
        }
    }
}
