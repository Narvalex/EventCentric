using EventCentric.Config;
using EventCentric.Messaging;
using EventCentric.NodeFactory.Log;
using EventCentric.Publishing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;

namespace EventCentric
{
    public class QueueNodeFactory<T>
    {
        public static INode CreateNode(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            var nodeName = NodeNameProvider.ResolveNameOf<T>();

            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();
            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventQueueDbContext> eventQueueDbContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var log = Logger.ResolvedLogger;

            var node = new QueueNode(nodeName, bus, log);

            var eventQueue = new EventQueue(nodeName, eventQueueDbContextFactory, serializer, time, guid, bus);
            var eventDao = new EventDao(eventQueueDbContextFactory);
            var eventBus = new EventBus(bus, log, eventQueue);
            var eventPublisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));

            var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 2, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);
            container.RegisterInstance<IGuidProvider>(guid);
            container.RegisterInstance<ITimeProvider>(time);

            return node;
        }

        /// <summary>
        /// Do not forget to: System.Data.Entity.Database.SetInitializer<TDbContext>(null);
        /// </summary>
        public static INode CreateCrudNode<TDbContext>(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true) where TDbContext : IEventQueueDbContext
        {
            var nodeName = NodeNameProvider.ResolveNameOf<T>();

            var eventStoreConfig = EventStoreConfig.GetConfig();
            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventQueueDbContext> eventQueueDbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var log = Logger.ResolvedLogger;

            var node = new QueueNode(nodeName, bus, log);

            var eventQueue = new CrudEventQueue(nodeName, eventQueueDbContextFactory, serializer, time, guid, bus);
            var eventDao = new EventDao(eventQueueDbContextFactory);
            var eventBus = new CrudEventBus(bus, log, eventQueue);
            var eventPublisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));

            var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 2, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));

            // Register for DI
            container.RegisterInstance<ICrudEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);
            container.RegisterInstance<IGuidProvider>(guid);
            container.RegisterInstance<ITimeProvider>(time);

            return node;
        }
    }
}
