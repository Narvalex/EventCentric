using EventCentric.Database;
using EventCentric.MicroserviceFactory;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class SingleMicroserviceInitializer
    {
        private static object _lockObject = new object();
        private static IMicroservice _microservice = null;
        private static bool isRunning = false;

        public static void Run(
            IUnityContainer container, Func<IMicroservice> microserviceFactory,
            bool useSignalRLog = true)
        {
            lock (_lockObject)
            {
                // Double checking
                if (_microservice != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                container = ContainerFactory.ResolveCommonDependenciesForMainContainer(container, useSignalRLog);
                _microservice = microserviceFactory.Invoke();
                _microservice.Start();
                isRunning = true;
            }
        }
    }
}
