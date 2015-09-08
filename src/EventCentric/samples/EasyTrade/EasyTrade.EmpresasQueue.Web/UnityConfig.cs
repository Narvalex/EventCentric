using EventCentric;
using EventCentric.Queueing;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;

namespace EasyTrade.EmpresasQueue.Web.App_Start
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

        private static object _lockObject = new object();
        private static INode _node = null;

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            lock (_lockObject)
            {
                if (_node != null)
                    return;

                _node = QueueNodeFactory<EmpresasQueueApp>
                        .CreateCrudNode<EmpresasQueueDbContext>(container);

                System.Data.Entity.Database.SetInitializer<EmpresasQueueDbContext>(null);

                _node.Start();

                var app = new EmpresasQueueApp(container.Resolve<ICrudEventBus>(), container.Resolve<IGuidProvider>());
                container.RegisterInstance<IEmpresasQueueApp>(app);
            }
        }
    }
}
