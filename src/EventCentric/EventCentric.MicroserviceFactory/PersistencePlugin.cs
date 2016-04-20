using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Persistence;
using EventCentric.Persistence.SqlServer;
using EventCentric.Persistence.SqlServerCe;
using EventCentric.Polling;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;

namespace EventCentric.MicroserviceFactory
{
    public enum PersistencePlugin
    {
        InMemory,
        SqlServer,
        SqlServerCe
    }

    public static class PersistencePluginResolver<TStream> where TStream : class, IEventSourced
    {
        public static IUnityContainer ResolvePersistence(IUnityContainer container, PersistencePlugin selectedPlugin, string microserviceName,
            string connectionString,                                            // Sql Based persistence
            Func<InMemoryEventStore<TStream>, InMemoryEventStore<TStream>> setupInMemoryPersistence)
        {
            switch (selectedPlugin)
            {
                case PersistencePlugin.InMemory:
                    var persitence = new InMemoryEventStore<TStream>(
                        microserviceName,
                        container.Resolve<IUtcTimeProvider>(),
                        container.Resolve<ITextSerializer>(),
                        container.Resolve<IGuidProvider>(),
                        container.Resolve<ILogger>());

                    setupInMemoryPersistence.Invoke(persitence);

                    container.RegisterInstance<ISubscriptionRepository>(persitence);
                    container.RegisterInstance<IEventDao>(persitence);
                    container.RegisterInstance<IEventStore<TStream>>(persitence);
                    break;

                case PersistencePlugin.SqlServer:
                    ResolveForSqlServer(container, microserviceName, connectionString);
                    break;

                case PersistencePlugin.SqlServerCe:
                    ResolveForSqlServerCe(container, microserviceName, connectionString);
                    break;

                default:
                    throw new InvalidOperationException("The selected plug in is not available");
            }

            return container;
        }

        private static void ResolveForSqlServer(IUnityContainer container, string microserviceName, string connectionString)
        {
            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, microserviceName, serializer, time);
            container.RegisterInstance<ISubscriptionRepository>(subscriptionRepository);

            var eventDao = new OptimizedEventDao(queueContextFactory, connectionString, microserviceName);
            container.RegisterInstance<IEventDao>(eventDao);

            var eventStore = new OptimizedEventStore<TStream>(microserviceName, serializer, connectionString, time, container.Resolve<IGuidProvider>(), container.Resolve<ILogger>());
            container.RegisterInstance<IEventStore<TStream>>(eventStore);
        }

        private static void ResolveForSqlServerCe(IUnityContainer container, string microserviceName, string connectionString)
        {
            Func<bool, EventStoreCeDbContext> storeContextFactory = isReadOnly => new EventStoreCeDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueCeDbContext> queueContextFactory = isReadOnly => new EventQueueCeDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, microserviceName, serializer, time);
            container.RegisterInstance<ISubscriptionRepository>(subscriptionRepository);

            var eventDao = new EventDao(queueContextFactory, microserviceName);
            container.RegisterInstance<IEventDao>(eventDao);

            var eventStore = new EventStore<TStream>(microserviceName, serializer, storeContextFactory, time, container.Resolve<IGuidProvider>(), container.Resolve<ILogger>());
            container.RegisterInstance<IEventStore<TStream>>(eventStore);
        }
    }
}
