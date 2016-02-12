using EventCentric.Database;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace EventCentric.MicroserviceFactory
{
    public class MultiMicroserviceInitializer
    {
        private static object _lockObject = new object();
        private static List<IMicroservice> _microservices = null;
        private static bool isRunning = false;

        public static void Run(IUnityContainer mainContainer, Func<List<IMicroservice>> microservicesFactory, bool useSignalRLog = true)
        {
            lock (_lockObject)
            {
                // Double checkingif
                if (_microservices != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                mainContainer = ContainerFactory.ResolveCommonDependenciesForMainContainer(mainContainer, useSignalRLog);
                _microservices = microservicesFactory.Invoke();
                _microservices.ForEach(s => s.Start());
                isRunning = true;
            }
        }
    }
}
