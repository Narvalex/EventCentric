using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Heartbeating;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Polling;
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
    public static class MicroserviceFactory<TStream, THandler>
        where TStream : class, IEventSourced
        where THandler : HandlerOf<TStream>
    {
        public static IMicroservice CreateEventProcessor(IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            bool isSubscriptor = true,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null,
            bool enableHeartbeatingListener = false)
        {
            var inMemoryPublisher = container.Resolve<IInMemoryEventPublisher>();
            var nodeName = EventSourceNameResolver.ResolveNameOf<TStream>();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();
            var guid = container.Resolve<IGuidProvider>();
            var log = container.Resolve<ILogger>();

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TStream>(nodeName, serializer, storeContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventPublisher>(publisher);

            var fsm = new EventProcessorMicroservice(nodeName, bus, log, isSubscriptor, enableHeartbeatingListener);
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
                var receiver = new MessageReceiver(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName, inMemoryPublisher);
                var poller = new Poller(bus, log, subscriptionRepository, receiver, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);

                var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
                container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);
            }

            if (enableHeartbeatingListener)
            {
                var heartbeatListener = new HeartbeatListener(nodeName, bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 10, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));
            }

            inMemoryPublisher.Register(publisher);
            return fsm;
        }

        public static IMicroservice CreateEventProcessorWithApp<TApp>(IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            bool isSubscriptor = true,
            Func<TApp> appFactory = null,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null,
            bool enableHeartbeatingListener = false)
                where TApp : ApplicationService
        {
            var inMemoryPublisher = container.Resolve<IInMemoryEventPublisher>();
            var nodeName = EventSourceNameResolver.ResolveNameOf<TStream>();
            var streamType = EventSourceNameResolver.ResolveNameOf<TApp>();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = container.Resolve<ILogger>();

            var serializer = container.Resolve<ITextSerializer>();

            var time = container.Resolve<IUtcTimeProvider>();

            var guid = container.Resolve<IGuidProvider>();
            container.RegisterInstance<IGuidProvider>(guid);

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TStream>(nodeName, serializer, storeContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventPublisher>(publisher);

            var fsm = new EventProcessorMicroservice(EventSourceNameResolver.ResolveNameOf<TStream>(), bus, log, isSubscriptor, enableHeartbeatingListener);
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
                var pollerPool = new MessageReceiver(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName, inMemoryPublisher);
                var poller = new Poller(bus, log, subscriptionRepository, pollerPool, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);

                var heartbeatEmitter = new HeartbeatEmitter(fsm, log, poller);
                container.RegisterInstance<HeartbeatEmitter>(heartbeatEmitter);
            }

            if (enableHeartbeatingListener)
            {
                var heartbeatListener = new HeartbeatListener(nodeName, bus, log, time, new TimeSpan(0, 1, 0), new TimeSpan(0, 10, 0), isReadonly => new HeartbeatDbContext(isReadonly, connectionString));
            }

            if (appFactory == null)
            {
                var constructor = typeof(TApp).GetConstructor(new[] { typeof(IGuidProvider), typeof(ILogger), typeof(string), typeof(int) });
                Ensure.CastIsValid(constructor, "Type TApp must have a valid constructor with the following signature: .ctor(IGuidProvider, ILogger, string, int)");
                container.RegisterInstance<TApp>((TApp)constructor.Invoke(new object[] { guid, log, streamType, eventStoreConfig.PushMaxCount }));
            }
            else
            {
                container.RegisterInstance<TApp>(appFactory.Invoke());
            }

            inMemoryPublisher.Register(publisher);
            inMemoryPublisher.Register(container.Resolve<TApp>());
            return fsm;
        }

        public static IMicroservice CreateDenormalizer<TDbContext>(IUnityContainer container,
            IEventStoreConfig eventStoreConfig,
            Func<IBus, ILogger, IEventStore<TStream>, THandler> processorFactory = null)
                where TDbContext : DbContext, IEventStoreDbContext
        {
            var nodeName = EventSourceNameResolver.ResolveNameOf<TStream>();

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


            var eventDao = new EventDao(queueContextFactory);

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
            var eventStore = new EventStore<TStream>(nodeName, serializer, dbContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);

            var bus = new Bus();
            container.RegisterInstance<IBus>(bus);

            var receiver = new MessageReceiver(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout), nodeName, container.Resolve<IInMemoryEventPublisher>());

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);

            var poller = new Poller(bus, log, subscriptionRepository, receiver, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            container.RegisterInstance<IMonitoredSubscriber>(poller);

            var publisher = new Publisher(nodeName, bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            container.RegisterInstance<IEventPublisher>(publisher);

            var fsm = new EventProcessorMicroservice(nodeName, bus, log, true, false);
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
