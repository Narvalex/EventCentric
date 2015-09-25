using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.NodeFactory.Factories;
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
    public class ProessorNodeFactory<TAggregate, THandler>
        where TAggregate : class, IEventSourced
        where THandler : EventProcessor<TAggregate>
    {
        public static INode CreateNode(IUnityContainer container, bool isSaga, bool setLocalTime = true, bool setSequentialGuid = true)
        {
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

            var eventStore = new EventStore<TAggregate>(serializer, storeContextFactory, time, guid, log);

            var bus = new Bus();

            var publisher = new Publisher<TAggregate>(bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            var fsm = new SagaNode(typeof(TAggregate).Name, bus, log);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var constructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
            Ensure.CastIsValid(constructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
            var processor = (THandler)constructor.Invoke(new object[] { bus, log, eventStore });

            // Register for DI
            container.RegisterInstance<IEventSource>(publisher);
            container.RegisterInstance<ILogger>(log);
            container.RegisterInstance<INode>(fsm);

            // For saga nodes
            if (isSaga)
            {
                var pollerConfig = PollerConfig.GetConfig();
                var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout));
                var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
                container.RegisterInstance<IMonitoredSubscriber>(poller);
            }

            return fsm;
        }

        /// <summary>
        /// Do not forget: System.Data.Entity.Database.SetInitializer<TDbContext>(null);
        /// </summary>
        public static INode CreateDenormalizerNode<TDbContext>(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true) where TDbContext : IEventStoreDbContext
        {
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
            var eventStore = new EventStore<TAggregate>(serializer, dbContextFactory, time, guid, log);

            var bus = new Bus();

            var http = new HttpLongPoller(bus, log, TimeSpan.FromMilliseconds(pollerConfig.Timeout));

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, serializer, time);
            var poller = new Poller(bus, log, subscriptionRepository, http, serializer, pollerConfig.BufferQueueMaxCount, pollerConfig.EventsToFlushMaxCount);
            var publisher = new Publisher<TAggregate>(bus, log, eventDao, eventStoreConfig.PushMaxCount, TimeSpan.FromMilliseconds(eventStoreConfig.LongPollingTimeout));
            var fsm = new SagaNode(typeof(TAggregate).Name, bus, log);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);

            var processorConstructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
            Ensure.CastIsValid(processorConstructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, ILogger, IEventStore<T>)");
            var processor = (THandler)processorConstructor.Invoke(new object[] { bus, log, eventStore });

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
