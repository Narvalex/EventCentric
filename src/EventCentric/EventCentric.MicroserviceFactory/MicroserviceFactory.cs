using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Heartbeating;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.MicroserviceFactory;
using EventCentric.Persistence;
using EventCentric.Persistence.SqlServer;
using EventCentric.Polling;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class MicroserviceFactory<TStream, THandler>
        where TStream : class, IEventSourced
        where THandler : HandlerOf<TStream>
    {
        private static string EnsureStreamCategoryNameIsValid(string name)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(name, "StreamCategoryName");
            return name;
        }

        public static IMicroservice CreateEventProcessor(
            string uniqueName,
            IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            PersistencePlugin selectedPlugin = PersistencePlugin.SqlServer,
            Func<InMemoryEventStore<TStream>, InMemoryEventStore<TStream>> setupInMemoryPersistence = null,
            bool isSubscriptor = true,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null)
        {
            var inMemoryPublisher = container.Resolve<IInMemoryEventPublisher>();

            var streamFullName = EnsureStreamCategoryNameIsValid(uniqueName);

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            PersistencePluginResolver<TStream>.ResolvePersistence(
                container, selectedPlugin, streamFullName, connectionString, setupInMemoryPersistence);

            var log = container.Resolve<ILogger>();

            var eventStore = container.Resolve<IEventStore<TStream>>();

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var publisher = new Publisher(streamFullName, bus, log, container.Resolve<IEventDao>(), eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IPollableEventSource>(publisher);

            var fsm = new MicroserviceHost(streamFullName, bus, log, isSubscriptor);
            container.RegisterInstance<IMicroservice>(fsm);

            // Processor factory
            if (processorFactory == null)
            {
                var constructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TStream>) });
                Ensure.CastIsValid(constructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (THandler)constructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            // For nodes that polls events from subscribed sources
            if (isSubscriptor)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var receiver = new LongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), streamFullName, inMemoryPublisher);
                var poller = new Poller(bus, log, container.Resolve<ISubscriptionRepository>(), receiver, container.Resolve<ITextSerializer>(), pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);

                var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
                container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);
            }

            inMemoryPublisher.Register(publisher);
            return fsm;
        }

        public static IMicroservice CreateEventProcessorWithApp<TApp>(
            string uniqueName,
            string appUniqueName,
            IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            PersistencePlugin selectedPlugin = PersistencePlugin.SqlServer,
            Func<InMemoryEventStore<TStream>, InMemoryEventStore<TStream>> setupInMemoryPersistence = null,
            bool isSubscriptor = true,
            Func<IGuidProvider, ILogger, string, int, TApp> appFactory = null,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null,
            Func<string, IBus, ILogger, IEventDao, int, TimeSpan, IPollableEventSource> publisherFactory = null)
                where TApp : ApplicationService
        {
            var inMemoryPublisher = container.Resolve<IInMemoryEventPublisher>();

            var streamFullName = EnsureStreamCategoryNameIsValid(uniqueName);
            var appFullName = EnsureStreamCategoryNameIsValid(appUniqueName);

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            PersistencePluginResolver<TStream>.ResolvePersistence(
                container, selectedPlugin, streamFullName, connectionString, setupInMemoryPersistence);

            var log = container.Resolve<ILogger>();

            var eventStore = container.Resolve<IEventStore<TStream>>();
            var eventDao = container.Resolve<IEventDao>();

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            IPollableEventSource publisher;
            if (publisherFactory == null)
            {
                publisher = new Publisher(streamFullName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
                container.RegisterInstance<IPollableEventSource>(publisher);
            }
            else
            {
                publisher = publisherFactory.Invoke(streamFullName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            }

            var fsm = new MicroserviceHost(streamFullName, bus, log, isSubscriptor);
            container.RegisterInstance<IMicroservice>(fsm);

            if (processorFactory == null)
            {
                var constructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TStream>) });
                Ensure.CastIsValid(constructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (THandler)constructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            // For nodes that polls events from subscribed sources

            if (isSubscriptor)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var pollerPool = new LongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), streamFullName, inMemoryPublisher);
                var poller = new Poller(bus, log, container.Resolve<ISubscriptionRepository>(), pollerPool, container.Resolve<ITextSerializer>(), pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);

                var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
                container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);
            }

            var guid = container.Resolve<IGuidProvider>();
            if (appFactory == null)
            {
                var constructor = typeof(TApp).GetConstructor(new[] { typeof(IGuidProvider), typeof(ILogger), typeof(string), typeof(int) });
                Ensure.CastIsValid(constructor, "Type TApp must have a valid constructor with the following signature: .ctor(IGuidProvider, ILogger, string, int)");
                container.RegisterInstance<TApp>((TApp)constructor.Invoke(new object[] { guid, log, appFullName, eventStoreConfig.PushMaxCount }));
            }
            else
            {
                container.RegisterInstance<TApp>(appFactory.Invoke(guid, log, appFullName, eventStoreConfig.PushMaxCount));
            }

            inMemoryPublisher.Register(publisher);
            inMemoryPublisher.Register(container.Resolve<TApp>());
            return fsm;
        }

        public static IMicroservice CreateDenormalizer<TDbContext>(
            string uniqueName,
            IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null)
                where TDbContext : DbContext, IEventStoreDbContext
        {
            var streamFullName = EnsureStreamCategoryNameIsValid(uniqueName);

            System.Data.Entity.Database.SetInitializer<TDbContext>(null);

            var pollerConfig = PollerConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = container.Resolve<ILogger>();

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();
            var guid = container.Resolve<IGuidProvider>();

            // Do not know why an Event Dao will need a denormalizer... and a Publisher!
            // The only events that can (and sould) be queries is 'ReadModelUpdated'.
            var eventDao = new EventDao(queueContextFactory, streamFullName);

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
            var eventStore = new EventStore<TStream>(streamFullName, serializer, dbContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var receiver = new LongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), streamFullName, container.Resolve<IInMemoryEventPublisher>());

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, streamFullName, serializer, time);

            var poller = new Poller(bus, log, subscriptionRepository, receiver, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            container.RegisterInstance<IMonitoredSubscriber>(poller);

            var publisher = new Publisher(streamFullName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IPollableEventSource>(publisher);

            var fsm = new MicroserviceHost(streamFullName, bus, log, true);
            container.RegisterInstance<IMicroservice>(fsm);

            if (processorFactory == null)
            {
                var processorConstructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TStream>) });
                Ensure.CastIsValid(processorConstructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (THandler)processorConstructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
            container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);

            return fsm;
        }
    }
}
