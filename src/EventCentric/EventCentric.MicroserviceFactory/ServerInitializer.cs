using EventCentric.Config;
using EventCentric.Database;
using EventCentric.Factory;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Data.Entity;

namespace EventCentric
{
    public static class ServerInitializer
    {
        private static object _lockObject = new object();
        private static IMicroservice _microservice = null;
        private static bool isRunning = false;

        public static void Run(
            IUnityContainer container,
            IEventStoreConfig config, Func<IUnityContainer,
            IEventStoreConfig, IMicroservice> microserviceFactory,
            bool useSignalRLog = true)
        {
            lock (_lockObject)
            {
                // Double checking
                if (_microservice != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                container = ResolveCommonDependencies(container, useSignalRLog);
                _microservice = microserviceFactory.Invoke(container, config);
                _microservice.Start();
                isRunning = true;
            }
        }

        public static void Run()
        {

        }

        private static IUnityContainer ResolveCommonDependencies(IUnityContainer container, bool useSignalRLog)
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var log = useSignalRLog ? (ILogger)SignalRLogger.ResolvedSignalRLogger : new ConsoleLogger();
            container.RegisterInstance<ILogger>(log);

            container.RegisterInstance<IInMemoryEventPublisher>(new InMemoryEventPublisher(log));

            var serializer = new JsonTextSerializer();
            container.RegisterInstance<ITextSerializer>(serializer);

            var time = new UtcTimeProvider() as IUtcTimeProvider;
            container.RegisterInstance<IUtcTimeProvider>(time);

            container.RegisterInstance<IGuidProvider>(new SequentialGuid());

            return container;
        }
    }
}
