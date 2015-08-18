using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;
using EventCentric.Publishing;
using EventCentric.Pulling;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;

namespace EventCentric
{
    public class NodeFactory<T> where T : class, IEventSourced
    {
        public static void CreateNode(IUnityContainer container, Func<IUnityContainer, EventProcessor<T>> processorFactory, bool setLocalTime = true, bool startNow = true)
        {
            var connectionProvider = ConnectionManager.GetConnectionProvider();
            var connectionString = connectionProvider.ConnectionString;

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;

            var streamDao = new StreamDao(() => new ReadOnlyStreamDbContext(connectionString));
            var subscriptionDao = new SubscriptionDao(() => new ReadOnlySubscriptionDbContext(connectionString));
            var subscriptionWriter = new SubscriptionWriter(() => new EventStoreDbContext(connectionString), time, serializer);

            var eventStore = new EventStore<T>(serializer, () => new EventStoreDbContext(connectionString), subscriptionWriter, time);

            var bus = new Bus();

            var publisher = new EventPublisher<T>(bus, streamDao);
            var puller = new EventPuller(bus, subscriptionDao, () => new SynchronousHttpClient(), serializer);
            var fsm = new FSM(bus);

            // Register Processor Dependencies
            container.RegisterInstance<IBus>(bus);
            container.RegisterInstance<IEventStore<T>>(eventStore);
            container.RegisterInstance<ISubscriptionWriter>(subscriptionWriter);
            // Register processor
            var processor = processorFactory.Invoke(container);

            // Register all in bus
            bus.Register(publisher, puller, processor, fsm);

            if (startNow)
                fsm.Start();

            // Register client bus
            var clientBus = new ClientBus(container.Resolve<IBus>());
            container.RegisterInstance<IClientBus>(clientBus);
        }
    }
}
