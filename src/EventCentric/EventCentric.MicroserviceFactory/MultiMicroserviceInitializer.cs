using EventCentric.Database;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Microservice;
using EventCentric.Publishing;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace EventCentric.MicroserviceFactory
{
    public class MultiMicroserviceInitializer
    {
        private static object _lockObject = new object();
        private static MultiMicroserviceContainer multiContainer = null;
        private static bool isRunning = false;

        public static void Run(IUnityContainer mainContainer, Func<List<IMicroservice>> microservicesFactory, bool useSignalRLog = true, params IEventPublisher[] ocassionallyConnectedSources)
        {
            lock (_lockObject)
            {
                // Double checkingif
                if (multiContainer != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                mainContainer = ContainerFactory.ResolveCommonDependenciesForMainContainer(mainContainer, useSignalRLog);
                multiContainer = new MultiMicroserviceContainer(
                    mainContainer.Resolve<IBus>(),
                    mainContainer.Resolve<ILogger>(),
                    microservicesFactory.Invoke());

                var inMemoryEventPublisher = mainContainer.Resolve<IInMemoryEventPublisher>();
                ocassionallyConnectedSources.ForEach(x => inMemoryEventPublisher.Register(x));

                multiContainer.Start();
                isRunning = true;
            }
        }
    }
}
