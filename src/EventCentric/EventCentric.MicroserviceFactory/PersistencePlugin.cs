using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Persistence;
using EventCentric.Persistence.SqlServer;
using EventCentric.Persistence.SqlServerCe;
using EventCentric.Polling;
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
            string connectionString,
            bool persistIncomingPayloads,
            // Sql Based persistence
            Func<InMemoryEventStore<TStream>, InMemoryEventStore<TStream>> setupInMemoryPersistence,
            Func<string, ITextSerializer, string, bool> consumerFilter)
        {
            switch (selectedPlugin)
            {
                case PersistencePlugin.InMemory:
                    var persistence = new InMemoryEventStore<TStream>(
                        microserviceName,
                        container.Resolve<IUtcTimeProvider>(),
                        container.Resolve<ITextSerializer>(),
                        container.Resolve<IGuidProvider>(),
                        container.Resolve<ILogger>(),
                        persistIncomingPayloads,
                        consumerFilter);

                    if (setupInMemoryPersistence != null)
                        setupInMemoryPersistence.Invoke(persistence);

                    container.RegisterInstance<ISubscriptionRepository>(persistence);
                    container.RegisterInstance<IEventStore<TStream>>(persistence);
                    break;

                case PersistencePlugin.SqlServer:
                    ResolveForSqlServer(container, microserviceName, connectionString, persistIncomingPayloads, consumerFilter);
                    break;

                case PersistencePlugin.SqlServerCe:
                    ResolveForSqlServerCe(container, microserviceName, connectionString, persistIncomingPayloads, consumerFilter);
                    break;

                default:
                    throw new InvalidOperationException("The selected plug in is not available");
            }

            return container;
        }

        private static void ResolveForSqlServer(IUnityContainer container, string microserviceName, string connectionString, bool persistIncomingPayloads, Func<string, ITextSerializer, string, bool> consumerFilter)
        {
            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();

            var eventStore = new OptimizedEventStore<TStream>(microserviceName, serializer, connectionString, time, container.Resolve<IGuidProvider>(), container.Resolve<ILogger>(), persistIncomingPayloads, consumerFilter);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);
            container.RegisterInstance<ISubscriptionRepository>(eventStore);
        }

        private static void ResolveForSqlServerCe(IUnityContainer container, string microserviceName, string connectionString, bool persistIncomingPayloads, Func<string, ITextSerializer, string, bool> consumerFilter)
        {
            Func<bool, EventStoreCeDbContext> storeContextFactory = isReadOnly =>
                new EventStoreCeDbContext(isReadOnly, connectionString);

            Func<bool, EventQueueCeDbContext> queueContextFactory = isReadOnly =>
                new EventQueueCeDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();

            var eventStore = new OrmEventStore<TStream>(microserviceName, serializer, storeContextFactory, time, container.Resolve<IGuidProvider>(), container.Resolve<ILogger>(), persistIncomingPayloads, consumerFilter);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);
            container.RegisterInstance<ISubscriptionRepository>(eventStore);
        }
    }
}
