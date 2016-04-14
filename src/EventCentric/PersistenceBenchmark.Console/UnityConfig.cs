using EventCentric;
using EventCentric.Config;
using EventCentric.EventSourcing;
using EventCentric.MicroserviceFactory;
using EventCentric.Persistence;
using Microsoft.Practices.Unity;
using PersistenceBenchmark.ConsoleHost;
using PersistenceBenchmark.PromotionsStream;
using System;
using System.Collections.Generic;

namespace PersistenceBenchmark
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {

        public static StatsMonitor StatsMonitor = new StatsMonitor();
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
        public static IUnityContainer GetConfiguredContainer(PersistencePlugin plugin)
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

            //SingleMicroserviceInitializer.Run(
            //    container, () => MicroserviceFactory<UserManagement, UserManagementHandler>
            //        .CreateEventProcessorWithApp<UserAppService>(container, userConfig),
            //    !_isConsoleApp);

            MultiMicroserviceInitializer.Run(container, () =>
            {
                var services = new List<IMicroservice>();

                UserContainer1 = ContainerFactory.ResolveDependenciesForNewChildContainer(container);
                services.Add(MicroserviceFactory<UserManagement, UserManagementHandler>
                    .CreateEventProcessorWithApp<UserAppService>("user1", "user1_app", UserContainer1, user1Config, plugin,
                        SetupInMemoryPersistence<UserManagement>));

                UserContainer2 = ContainerFactory.ResolveDependenciesForNewChildContainer(container);
                services.Add(MicroserviceFactory<UserManagement, UserManagementHandler>
                    .CreateEventProcessorWithApp<UserAppService>("user2", "user2_app", UserContainer2, user2Config, plugin,
                        SetupInMemoryPersistence<UserManagement>));

                PromotionsContainer = ContainerFactory.ResolveDependenciesForNewChildContainer(container);
                services.Add(MicroserviceFactory<Promotions, PromotionsHandler>.
                    CreateEventProcessor("promo", PromotionsContainer, promotionsConfig, plugin,
                        SetupInMemoryPersistence<Promotions>));

                return services;
            }, false, Program.VerboseIsEnabled);
        }

        private static InMemoryEventStore<T> SetupInMemoryPersistence<T>(InMemoryEventStore<T> store) where T : class, IEventSourced
        {
            store.Setup(DbManager.GetSubscriptions());
            StatsMonitor.Add(store);
            return store;
        }

        public static IUnityContainer UserContainer1 { get; private set; }
        public static IUnityContainer UserContainer2 { get; private set; }
        public static IUnityContainer PromotionsContainer { get; private set; }

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
