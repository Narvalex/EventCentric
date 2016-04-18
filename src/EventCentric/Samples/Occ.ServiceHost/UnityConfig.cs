using EventCentric;
using EventCentric.Config;
using EventCentric.Publishing;
using Microsoft.Practices.Unity;
using Occ.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Occ.ServiceHost.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
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
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            var config = new EventStoreConfig();
            proxies.Add("OccClient1", new OcassionallyConnectedSource("OccClient1"));
            proxies.Add("OccClient2", new OcassionallyConnectedSource("OccClient2"));

            SingleMicroserviceInitializer.Run(
            container: container,
            useSignalRLog: true,
            ocassionallyConnectedSources: proxies.Values.ToList(),
            microserviceFactory: () =>
                MicroserviceFactory<ItemServer, ItemServerHandler>
                .CreateEventProcessor(
                    uniqueName: "OccServer",
                    container: container,
                    eventStoreConfig: config));
        }

        private static Dictionary<string, OcassionallyConnectedSource> proxies = new Dictionary<string, OcassionallyConnectedSource>();

        public static OcassionallyConnectedSource GetClientProxy(string name) => proxies[name];
    }

    public class EventStoreConfig : IEventStoreConfig
    {
        public EventStoreConfig()
        {
            this.ConnectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
            this.Token = ConfigurationManager.AppSettings["token"]; ;
        }

        public string ConnectionString { get; }

        public double LongPollingTimeout => 60000;

        public int PushMaxCount => 100;

        public string Token { get; }
    }
}
