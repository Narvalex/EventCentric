using EventCentric;
using EventCentric.Config;
using EventCentric.MicroserviceFactory;
using Microsoft.Practices.Unity;
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
        private static bool _isConsoleApp;

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
        public static IUnityContainer GetConfiguredContainer(bool isConsoleApp)
        {
            _isConsoleApp = isConsoleApp;
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            var userConfig = EventStoreConfig.GetConfig();
            var promotionsConfig = new DummyEventStoreConfig("server = (local); Database = PromotionsDb; User Id = sa; pwd = 123456", 60000, 100, "123");

            //DbManager.CreateDbs(promotionsConfig.ConnectionString);

            //SingleMicroserviceInitializer.Run(
            //    container, () => MicroserviceFactory<UserManagement, UserManagementHandler>
            //        .CreateEventProcessorWithApp<UserAppService>(container, userConfig),
            //    !_isConsoleApp);

            MultiMicroserviceInitializer.Run(container, () =>
            {
                var services = new List<IMicroservice>();

                var userContainer = ContainerFactory.ResolveDependenciesForNewChildContainer(container);
                services.Add(MicroserviceFactory<UserManagement, UserManagementHandler>.CreateEventProcessorWithApp<UserAppService>(userContainer, userConfig));
                UserContainer = userContainer;

                var promotionsContainer = ContainerFactory.ResolveDependenciesForNewChildContainer(container);
                services.Add(MicroserviceFactory<Promotions, PromotionsHandler>.CreateEventProcessor(container, promotionsConfig));
                PromotionsContainer = promotionsContainer;

                return services;
            },
            !_isConsoleApp);
        }

        public static IUnityContainer UserContainer { get; private set; }
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

            public string ConnectionString { get; }

            public double LongPollingTimeout { get; }

            public int PushMaxCount { get; }
            public string Token { get; }
        }
    }
}
