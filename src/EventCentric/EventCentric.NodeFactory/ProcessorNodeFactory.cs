﻿using EventCentric.Config;
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

namespace EventCentric
{
    public class ProessorNodeFactory<TAggregate, TProcessor>
        where TAggregate : class, IEventSourced
        where TProcessor : EventProcessor<TAggregate>
    {
        public static INode CreateNode(IUnityContainer container, bool hasSubscription, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            var nodeName = NodeNameProvider.ResolveNameOf<TAggregate>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TAggregate>(nodeName, serializer, storeContextFactory, time, guid, log);

            var bus = new Bus();

            var publisher = new Publisher<TAggregate>(bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            var fsm = new ProcessorNode(nodeName, bus, log);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

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

            // Register for DI
            container.RegisterInstance<IEventSource>(publisher);
            container.RegisterInstance<ILogger>(log);
            container.RegisterInstance<INode>(fsm);

            // For nodes that polls events from subscribed sources
            if (hasSubscription)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout));
                var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);
            }

            return fsm;
        }

        /// <summary>
        /// Node with in process app.
        /// </summary>
        /// <typeparam name="TApp">In process app.</typeparam>
        /// <returns></returns>
        public static INode CreateNodeWithApp<TApp>(IUnityContainer container, bool hasSubscription, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            var nodeName = NodeNameProvider.ResolveNameOf<TAggregate>();
            var appName = NodeNameProvider.ResolveNameOf<TApp>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var eventDao = new EventDao(queueContextFactory);

            var eventStore = new EventStore<TAggregate>(nodeName, serializer, storeContextFactory, time, guid, log);

            var bus = new Messaging.Bus();

            var publisher = new Publisher<TAggregate>(bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            var fsm = new ProcessorNode(NodeNameProvider.ResolveNameOf<TAggregate>(), bus, log);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

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
            if (hasSubscription)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout));
                var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);
            }

            // Event Queue feature
            var eventQueue = new InMemoryEventQueue(appName, guid, bus);
            var eventBus = new EventBus(bus, log, eventQueue);

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(publisher);
            container.RegisterInstance<ILogger>(log);
            container.RegisterInstance<INode>(fsm);

            return fsm;
        }

        /// <summary>
        /// Do not forget: System.Data.Entity.Database.SetInitializer<TDbContext>(null);
        /// </summary>
        public static INode CreateDenormalizerNode<TDbContext>(IUnityContainer container, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null, bool setLocalTime = true, bool setSequentialGuid = true) where TDbContext : IEventStoreDbContext
        {
            var nodeName = NodeNameProvider.ResolveNameOf<TAggregate>();

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var eventStoreConfig = EventStoreConfig.GetConfig();
            var pollerConfig = PollerConfig.GetConfig();

            var connectionString = eventStoreConfig.ConnectionString;

            AuthorizationFactory.SetToken(eventStoreConfig);

            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var log = Logger.ResolvedLogger;
            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;


            var eventDao = new EventDao(queueContextFactory);

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(bool), typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(bool, string)");
            Func<bool, IEventStoreDbContext> dbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { isReadOnly, connectionString });
            var eventStore = new EventStore<TAggregate>(nodeName, serializer, dbContextFactory, time, guid, log);

            var bus = new Messaging.Bus();

            var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout));

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            var publisher = new Publisher<TAggregate>(bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            var fsm = new ProcessorNode(nodeName, bus, log);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

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

            // Register for DI
            container.RegisterInstance<IEventSource>(publisher);
            container.RegisterInstance<IEventStoreConfig>(eventStoreConfig);
            container.RegisterInstance<ILogger>(log);
            container.RegisterInstance<INode>(fsm);
            container.RegisterInstance<IMonitoredSubscriber>(poller);

            return fsm;
        }
    }
}
