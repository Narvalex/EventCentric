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
        where THandler : Handler<TStream>
    {
        private static string EnsureStreamCategoryNameIsValid(string name)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(name, "StreamCategoryName");
            return name;
        }

        public static Func<IMicroservice> CreateEventProcessor(
            string uniqueName,
            IEventStoreConfig eventStoreConfig = null,
            IPollerConfig pollerConfig = null,
            PersistencePlugin selectedPlugin = PersistencePlugin.SqlServer,
            bool persistIncomingPayloads = true,
            Func<InMemoryEventStore<TStream>, InMemoryEventStore<TStream>> setupInMemoryPersistence = null,
            Func<string, string, bool> consumerFilter = null,
            bool isSubscriptor = true,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null,
            Func<string, IEventStore, IBus, ILogger, int, TimeSpan, IPollableEventSource> publisherFactory = null)
        {
            return () =>
            {
                var streamFullName = EnsureStreamCategoryNameIsValid(uniqueName);

                var container = EventSystem.ResolveNewChildContainerAndRegisterInMemorySubscriptions(uniqueName);
                var inMemoryPublisher = container.Resolve<IInMemoryEventPublisher>();

                eventStoreConfig = ConfigResolver.ResolveConfig(eventStoreConfig);

                var connectionString = eventStoreConfig.ConnectionString;

                AuthorizationFactory.SetToken(eventStoreConfig);

                PersistencePluginResolver<TStream>.ResolvePersistence(
                    container, selectedPlugin, streamFullName, connectionString, persistIncomingPayloads, setupInMemoryPersistence, consumerFilter);

                var log = container.Resolve<ILogger>();

                var eventStore = container.Resolve<IEventStore<TStream>>();

                var bus = new Bus();
                container.RegisterInstance<IBus>(bus);

                IPollableEventSource publisher;
                if (publisherFactory == null)
                {
                    publisher = new Publisher(streamFullName, eventStore, bus, log, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
                    container.RegisterInstance<IPollableEventSource>(publisher);
                }
                else
                {
                    publisher = publisherFactory.Invoke(streamFullName, eventStore, bus, log, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
                }

                var fsm = new MicroserviceHost(streamFullName, bus, log, isSubscriptor);
                container.RegisterInstance<IMicroservice>(fsm);

                // Processor factory
                THandler processor;
                if (processorFactory == null)
                {
                    var constructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TStream>) });
                    Ensure.CastIsValid(constructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                    processor = (THandler)constructor.Invoke(new object[] { bus, log, eventStore });
                }
                else
                {
                    processor = processorFactory.Invoke(bus, log, eventStore);
                }
                container.RegisterInstance<IProcessor<TStream>>(processor);

                // For nodes that polls events from subscribed sources
                if (isSubscriptor)
                {
                    pollerConfig = ConfigResolver.ResolveConfig(pollerConfig);
                    var receiver = new LongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), streamFullName, inMemoryPublisher);
                    var poller = new Poller(bus, log, inMemoryPublisher, container.Resolve<ISubscriptionRepository>(), receiver, container.Resolve<ITextSerializer>(), pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                    container.RegisterInstance<IMonitoredSubscriber>(poller);

                    var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
                    container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);
                }

                inMemoryPublisher.Register(publisher);
                return fsm;
            };
        }

        public static Func<IMicroservice> CreateDenormalizer<TDbContext>(
            string uniqueName,
            IEventStoreConfig eventStoreConfig = null,
            IPollerConfig pollerConfig = null,
            bool persistIncomingPayloads = true,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null)
                where TDbContext : DbContext, IEventStoreDbContext
        {
            return () =>
            {
                var streamFullName = EnsureStreamCategoryNameIsValid(uniqueName);

                var container = EventSystem.ResolveNewChildContainerAndRegisterInMemorySubscriptions(streamFullName);

                System.Data.Entity.Database.SetInitializer<TDbContext>(null);

                pollerConfig = ConfigResolver.ResolveConfig(pollerConfig);
                eventStoreConfig = ConfigResolver.ResolveConfig(eventStoreConfig);

                var connectionString = eventStoreConfig.ConnectionString;

                AuthorizationFactory.SetToken(eventStoreConfig);

                Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
                Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

                var log = container.Resolve<ILogger>();

                var serializer = container.Resolve<ITextSerializer>();
                var time = container.Resolve<IUtcTimeProvider>();
                var guid = container.Resolve<IGuidProvider>();

                // Do not know why an EventStore will need a denormalizer... and a Publisher!
                // The only events that can (and sould) be queries is 'ReadModelUpdated'.

                var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
                Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
                Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
                var eventStore = new OrmEventStore<TStream>(streamFullName, serializer, dbContextFactory, time, guid, log, persistIncomingPayloads, null);
                container.RegisterInstance<IEventStore<TStream>>(eventStore);

                var bus = new Bus();
                container.RegisterInstance<IBus>(bus);

                var receiver = new LongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), streamFullName, container.Resolve<IInMemoryEventPublisher>());

                var subscriptionRepository = new SubscriptionRepository(storeContextFactory, streamFullName, serializer, time);

                var poller = new Poller(bus, log, container.Resolve<IInMemoryEventPublisher>(), subscriptionRepository, receiver, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);

                var publisher = new Publisher(streamFullName, eventStore, bus, log, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
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
            };
        }
    }
}
