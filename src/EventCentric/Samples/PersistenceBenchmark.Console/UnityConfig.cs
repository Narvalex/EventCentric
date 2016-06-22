using EventCentric;
using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.MicroserviceFactory;
using EventCentric.Persistence;
using EventCentric.Serialization;
using Microsoft.Practices.Unity;
using PersistenceBenchmark.ConsoleHost;
using PersistenceBenchmark.PromotionsStream;
using System;

namespace PersistenceBenchmark
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        private static bool persistIncomingEvents = false;

        public static StatsMonitor StatsMonitor;
        private static PersistencePlugin plugin;

        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer InitializeMainContainer(PersistencePlugin plugin)
        {
            UnityConfig.plugin = plugin;
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            var baseConfig = EventStoreConfig.GetConfig();

            var promotionsConfig = new DummyEventStoreConfig(DbManager.FixedConnectionstring, baseConfig);
            var user1Config = new DummyEventStoreConfig(DbManager.FixedConnectionstring, baseConfig);
            var user2Config = new DummyEventStoreConfig(DbManager.FixedConnectionstring, baseConfig);
            StatsMonitor = new StatsMonitor();

            EventSystem.Create(
               MicroserviceFactory<UserManagement, UserManagementHandler>
                        .CreateEventProcessor("user1", user1Config, null, plugin, persistIncomingEvents, false,
                            SetupInMemoryPersistence<UserManagement>),

               MicroserviceFactory<UserManagement, UserManagementHandler>
                    .CreateEventProcessor("user2", user2Config, null, plugin, persistIncomingEvents, false,
                        SetupInMemoryPersistence<UserManagement>),

               MicroserviceFactory<Promotions, PromotionsHandler>.
                    CreateEventProcessor("promo", promotionsConfig, null, plugin, persistIncomingEvents, false,
                        SetupInMemoryPersistence<Promotions>,
                        (consumer, serializer, payload) =>
                        {
                            var e = serializer.Deserialize<IEvent>(payload);
                            if (e is FreePointsRewardedToUser)
                                return false;
                            return true;
                        }));
        }

        private static InMemoryEventStore<T> SetupInMemoryPersistence<T>(InMemoryEventStore<T> store) where T : class, IEventSourced
        {
            store.Setup(DbManager.GetSubscriptions());
            StatsMonitor.Add(store);
            return store;
        }

        public class DummyEventStoreConfig : IEventStoreConfig
        {
            public DummyEventStoreConfig(string connectionString, double LongPollingTimeout, int PushMaxCount, string Token)
            {
                this.ConnectionString = connectionString;
                this.LongPollingTimeout = LongPollingTimeout;
                this.PushMaxCount = PushMaxCount;
                this.Token = Token;
            }

            public DummyEventStoreConfig(string connectionString, IEventStoreConfig baseConfig)
            {
                this.ConnectionString = connectionString;
                this.LongPollingTimeout = baseConfig.LongPollingTimeout;
                this.PushMaxCount = baseConfig.PushMaxCount;
                this.Token = baseConfig.Token;
            }

            public string ConnectionString { get; }

            public double LongPollingTimeout { get; }

            public int PushMaxCount { get; }

            public string Token { get; }
        }
    }
}
