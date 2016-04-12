using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Persistence.SqlServer;
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
        SqlServer
    }

    public static class PersistencePluginResolver<TStream> where TStream : class, IEventSourced
    {
        public static IUnityContainer ResolvePersistence(IUnityContainer container, PersistencePlugin selectedPlugin, string microserviceName, string connectionString)
        {
            switch (selectedPlugin)
            {
                case PersistencePlugin.InMemory:
                    ResolveForInMemoryPersistance(container, microserviceName);
                    break;

                case PersistencePlugin.SqlServer:
                    ResolveForSqlServer(container, microserviceName, connectionString);
                    break;

                default:
                    throw new InvalidOperationException("The selected plug in is not available");
            }

            return container;
        }

        private static void ResolveForInMemoryPersistance(IUnityContainer container, string microserviceName)
        {

        }

        private static void ResolveForSqlServer(IUnityContainer container, string microserviceName, string connectionString)
        {
            Func<bool, EventStoreDbContext> storeContextFactory = isReadOnly => new EventStoreDbContext(isReadOnly, connectionString);
            Func<bool, EventQueueDbContext> queueContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = container.Resolve<ITextSerializer>();
            var time = container.Resolve<IUtcTimeProvider>();
            var log = container.Resolve<ILogger>();
            var guid = container.Resolve<IGuidProvider>();

            var subscriptionRepository = new SubscriptionRepository(storeContextFactory, microserviceName, serializer, time);
            container.RegisterInstance<ISubscriptionRepository>(subscriptionRepository);

            var eventDao = new EventDao(queueContextFactory, microserviceName);
            container.RegisterInstance<IEventDao>(eventDao);

            var eventStore = new EventStore<TStream>(microserviceName, serializer, storeContextFactory, time, guid, log);
            container.RegisterInstance<IEventStore<TStream>>(eventStore);
        }
    }
}
