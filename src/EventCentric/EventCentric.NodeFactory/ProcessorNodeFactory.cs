using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.NodeFactory.Log;
using EventCentric.Polling;
using EventCentric.Processing;
using EventCentric.Publishing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class ProcessorNodeFactory<TAggregate, TProcessor>
        where TAggregate : class, IEventSourced
        where TProcessor : EventProcessor<TAggregate>
    {
        public static INode CreateNode(IUnityContainer container, bool isSubscriptor = true, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool enableHeartbeatingListener = false, bool setSequentialGuid = true)
        {
            var nodeName = NodeNameResolver.ResolveNameOf<TAggregate>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            container.RegisterInstance<ILogger>(log);

            var serializer = new JsonTextSerializer();
            container.RegisterInstance<ITextSerializer>(serializer);

            var time = new UtcTimeProvider() as IUtcTimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TAggregate>(nodeName, serializer, storeContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventSource>(publisher);

            var fsm = new ProcessorNode(nodeName, bus, log, isSubscriptor, enableHeartbeatingListener);
            container.RegisterInstance<INode>(fsm);

            // Processor factory
            if (processorFactory == null)
            {
                var constructor = typeof(TProcessor).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
                Ensure.CastIsValid(constructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (TProcessor)constructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            // For nodes that polls events from subscribed sources
            if (isSubscriptor)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName);
                var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);
            }

            if (enableHeartbeatingListener)
            {
                var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 10, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));
            }

            return fsm;
        }

        public static INode CreateNodeWithApp<TApp>(IUnityContainer container, bool isSubscriptor = true, Func<TApp> appFactory = null, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool enableHeartbeatingListener = false, bool setSequentialGuid = true)
            where TApp : ApplicationService
        {
            var nodeName = NodeNameResolver.ResolveNameOf<TAggregate>();
            var streamType = NodeNameResolver.ResolveNameOf<TApp>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            container.RegisterInstance<ILogger>(log);

            var serializer = new JsonTextSerializer();
            container.RegisterInstance<ITextSerializer>(serializer);

            var time = new UtcTimeProvider() as IUtcTimeProvider;
            container.RegisterInstance<IUtcTimeProvider>(time);

            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;
            container.RegisterInstance<IGuidProvider>(guid);

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TAggregate>(nodeName, serializer, storeContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventSource>(publisher);

            // Event Queue feature
            var eventQueue = new InMemoryEventQueue(streamType, guid, bus, time);

            var eventBus = new EventBus(bus, log, eventQueue);
            container.RegisterInstance<IEventBus>(eventBus);

            var fsm = new ProcessorNode(NodeNameResolver.ResolveNameOf<TAggregate>(), bus, log, isSubscriptor, enableHeartbeatingListener);
            container.RegisterInstance<INode>(fsm);

            if (processorFactory == null)
            {
                var constructor = typeof(TProcessor).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
                Ensure.CastIsValid(constructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (TProcessor)constructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            // For nodes that polls events from subscribed sources

            if (isSubscriptor)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName);
                var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);
            }

            if (enableHeartbeatingListener)
            {
                var heartbeatListener = new HeartbeatListener(bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 10, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));
            }

            if (appFactory == null)
            {
                var constructor = typeof(TApp).GetConstructor(new[] { typeof(IEventBus), typeof(IGuidProvider), typeof(ILogger) });
                Ensure.CastIsValid(constructor, "Type TApp must have a valid constructor with the following signature: .ctor(IEventBus, IGuidProvider, ILogger)");
                container.RegisterInstance<TApp>((TApp)constructor.Invoke(new object[] { eventBus, guid, log }));
            }
            else
            {
                container.RegisterInstance<TApp>(appFactory.Invoke());
            }

            return fsm;
        }

        public static INode CreateDenormalizerNode<TDbContext>(IUnityContainer container, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool setSequentialGuid = true)
            where TDbContext : DbContext, IEventStoreDbContext
        {
            var nodeName = NodeNameResolver.ResolveNameOf<TAggregate>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);
            System.Data.Entity.Database.SetInitializer<TDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();
            container.RegisterInstance<IEventStoreConfig>(eventStoreConfig);

            var pollerConfig = PollerConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            container.RegisterInstance<ILogger>(log);

            var serializer = new JsonTextSerializer();
            var time = new UtcTimeProvider() as IUtcTimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;


            var eventDao = new EventDao(queueContextFactory);

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
            var eventStore = new EventStore<TAggregate>(nodeName, serializer, dbContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName);

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);

            var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            container.RegisterInstance<IMonitoredSubscriber>(poller);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventSource>(publisher);

            var fsm = new ProcessorNode(nodeName, bus, log, true, false);
            container.RegisterInstance<INode>(fsm);

            if (processorFactory == null)
            {
                var processorConstructor = typeof(TProcessor).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
                Ensure.CastIsValid(processorConstructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (TProcessor)processorConstructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            return fsm;
        }

        public static INode CreateDenormalizerNodeWithDao<TEventuallyConsistentDbContext, TDao>(IUnityContainer container, Func<TEventuallyConsistentDbContext> eventuallyConsistentDbContextFactory = null, Func<Func<TEventuallyConsistentDbContext>, TDao> daoFactory = null, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool setSequentialGuid = true)
           where TEventuallyConsistentDbContext : EventuallyConsistentDbContext, IEventStoreDbContext
        {
            var nodeName = NodeNameResolver.ResolveNameOf<TAggregate>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);
            System.Data.Entity.Database.SetInitializer<TEventuallyConsistentDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();
            container.RegisterInstance<IEventStoreConfig>(eventStoreConfig);

            var pollerConfig = PollerConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            container.RegisterInstance<ILogger>(log);

            var serializer = new JsonTextSerializer();
            var time = new UtcTimeProvider() as IUtcTimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;


            var eventDao = new EventDao(queueContextFactory);

            var dbContextConstructor = typeof(TEventuallyConsistentDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TEventuallyConsistentDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
            var eventStore = new EventStore<TAggregate>(nodeName, serializer, dbContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName);

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);

            var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            container.RegisterInstance<IMonitoredSubscriber>(poller);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventSource>(publisher);

            var fsm = new ProcessorNode(nodeName, bus, log, true, false);
            container.RegisterInstance<INode>(fsm);

            if (processorFactory == null)
            {
                var processorConstructor = typeof(TProcessor).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
                Ensure.CastIsValid(processorConstructor, "Type TProcessor must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
                var processor = (TProcessor)processorConstructor.Invoke(new object[] { bus, log, eventStore });
            }
            else
            {
                var processor = processorFactory.Invoke(bus, log, eventStore);
            }

            if (eventuallyConsistentDbContextFactory == null)
            {
                var eventuallyConsistentDbContextConstructor = typeof(TEventuallyConsistentDbContext).GetConstructor(new[] { typeof(TimeSpan), typeof(bool), typeof(string) });
                Ensure.CastIsValid(eventuallyConsistentDbContextConstructor, "Type TEventuallyConsistentDbContext must have a constructor with the following signature: ctor(TimeSpan, bool, string)");
                eventuallyConsistentDbContextFactory = () => (TEventuallyConsistentDbContext)eventuallyConsistentDbContextConstructor.Invoke(new object[] { TimeSpan.FromSeconds(90), true, connectionString });
            }

            TDao dao;
            if (daoFactory == null)
            {
                var daoConstructor = typeof(TDao).GetConstructor(new[] { typeof(Func<TEventuallyConsistentDbContext>) });
                Ensure.CastIsValid(daoConstructor, "Type TDao must have a valid constructor with the following signature: .ctor(Func<TEventuallyConsistentDbContext>)");
                dao = (TDao)daoConstructor.Invoke(new object[] { eventuallyConsistentDbContextFactory });
            }
            else
            {
                dao = daoFactory.Invoke(eventuallyConsistentDbContextFactory);
            }

            container.RegisterInstance<TDao>(dao);

            return fsm;
        }
    }
}
