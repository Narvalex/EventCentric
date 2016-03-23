using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.MicroserviceFactory;
using EventCentric.Publishing;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
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
            bool useSignalRLog = true, IEnumerable<IEventPublisher> ocassionallyConnectedSources = null)
        {
            lock (_lockObject)
            {
                // Double checking
                if (_microservice != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                container = ContainerFactory.ResolveCommonDependenciesForMainContainer(container, useSignalRLog);
                _microservice = microserviceFactory.Invoke();

                if (ocassionallyConnectedSources != null)
                {
                    var inMemoryEventPublisher = container.Resolve<IInMemoryEventPublisher>();
                    ocassionallyConnectedSources.ForEach(x => inMemoryEventPublisher.Register(x));
                }

                _microservice.Start();
                isRunning = true;
            }
        }
    }
}
