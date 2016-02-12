using EventCentric;
using EventCentric.Config;
using Microsoft.Practices.Unity;
using System;

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
            DbManager.CreateDb();

            ServerInitializer.Run(
                container,
                EventStoreConfig.GetConfig(), (c, conf) => MicroserviceFactory<UserManagement, UserManagementHandler>
                    .CreateEventProcessorWithApp<AppService>(c, conf),
                !_isConsoleApp);
        }
    }
}
