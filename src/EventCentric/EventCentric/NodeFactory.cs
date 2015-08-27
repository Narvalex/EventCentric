using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Polling;
using EventCentric.Processing;
using EventCentric.Publishing;
using EventCentric.Pulling;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public class NodeFactory<TAggregate, THandler>
        where TAggregate : class, IEventSourced
        where THandler : EventProcessor<TAggregate>
    {
        public static INode CreateNode(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var connectionProvider = ConnectionManager.GetConnectionProvider();
            var connectionString = connectionProvider.ConnectionString;

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var streamDao = new OldStreamDao(() => new EventQueueDbContext(connectionString));
            var subscriptionDao = new SubscriptionDao(() => new EventQueueDbContext(connectionString));
            var subscriptionWriter = new SubscriptionInboxWriter(() => new EventStoreDbContext(connectionString), time, serializer);
            var subscriptionRepository = new SubscriptionRepository(() => new EventStoreDbContext(connectionString));
            var eventDao = new EventDao(() => new EventQueueDbContext(connectionString));

            var eventStore = new EventStore<TAggregate>(serializer, () => new EventStoreDbContext(connectionString), subscriptionWriter, time, guid);

            var bus = new Bus();

            var http = new HttpPoller(bus);

            var buffer = new BufferPool(bus, subscriptionRepository, http);
            var publisher = new EventPublisher<TAggregate>(bus, eventDao);
            var pollster = new EventPollster(bus, buffer);
            var fsm = new Node(bus);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);
            container.RegisterInstance<ISubscriptionInboxWriter>(subscriptionWriter);

            var constructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(IEventStore<TAggregate>), typeof(ISubscriptionInboxWriter) });
            Ensure.CastIsValid(constructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, IEventStore<T>, ISubscriptionInboxWriter)");
            var processor = (THandler)constructor.Invoke(new object[] { bus, eventStore, subscriptionWriter });

            // Register for DI
            container.RegisterInstance<IEventSource>(publisher);

            return fsm;
        }

        public static INode CreateDenormalizerNode<TDbContext>(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true) where TDbContext : IEventStoreDbContext
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var connectionProvider = ConnectionManager.GetConnectionProvider();
            var connectionString = connectionProvider.ConnectionString;

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var streamDao = new OldStreamDao(() => new EventQueueDbContext(connectionString));
            var subscriptionDao = new SubscriptionDao(() => new EventQueueDbContext(connectionString));
            var subscriptionWriter = new SubscriptionInboxWriter(() => new EventStoreDbContext(connectionString), time, serializer);
            var eventDao = new EventDao(() => new EventQueueDbContext(connectionString));

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(string)");
            Func<IEventStoreDbContext> dbContextFactory = () => (TDbContext)dbContextConstructor.Invoke(new object[] { connectionString });
            var eventStore = new EventStore<TAggregate>(serializer, dbContextFactory, subscriptionWriter, time, guid);

            var bus = new Bus();

            var publisher = new EventPublisher<TAggregate>(bus, eventDao);
            var puller = new OldEventPullerPerStream(bus, subscriptionDao, subscriptionWriter, new OldHttpPoller(), serializer);
            var fsm = new Node(bus);

            // Register processor dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<TAggregate>>(eventStore);
            container.RegisterInstance<ISubscriptionInboxWriter>(subscriptionWriter);

            var processorConstructor = typeof(THandler).GetConstructor(new[] { typeof(IBus), typeof(IEventStore<TAggregate>), typeof(ISubscriptionInboxWriter) });
            Ensure.CastIsValid(processorConstructor, "Type THandler must have a valid constructor with the following signature: .ctor(IBus, IEventStore<T>, ISubscriptionInboxWriter)");
            var processor = (THandler)processorConstructor.Invoke(new object[] { bus, eventStore, subscriptionWriter });

            // Register for DI
            container.RegisterInstance<IEventSource>(publisher);

            return fsm;
        }
    }
}
