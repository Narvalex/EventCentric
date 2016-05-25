using EventCentric.Database;
using EventCentric.Factory;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Microservice;
using EventCentric.Persistence.SqlServer;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EventCentric.MicroserviceFactory
{
    public class EventSystemSetup
    {
        internal EventSystemSetup() { }

        public EventSystemSetup UseSignalRLog(bool enable = true)
        {
            EventSystem.UseSignalRLog = enable;
            return this;
        }

        public EventSystemSetup EnableVerboseLogging(bool enable = true)
        {
            EventSystem.Verbose = enable;
            return this;
        }

        public EventSystemSetup RegisterOcassionallyConnectedSources(params IPollableEventSource[] sources)
        {
            EventSystem.OcassionallyConnectedSources = sources;
            return this;
        }

        public void Create(params Func<IMicroservice>[] microservicesFactories)
        {
            EventSystem.Create(microservicesFactories);
        }
    }

    public class EventSystem
    {
        private static readonly object _lockObject = new object();
        private static MultiMicroserviceContainer multiContainer = null;
        private static bool isRunning = false;

        internal static bool UseSignalRLog = false;
        internal static bool Verbose = false;
        internal static IPollableEventSource[] OcassionallyConnectedSources = null;


        private static IUnityContainer mainContainer;
        private static readonly Dictionary<string, IUnityContainer> childContainers = new Dictionary<string, IUnityContainer>();

        private EventSystem() { }

        public static EventSystemSetup Setup() => new EventSystemSetup();

        /// <summary>
        /// Creates a new event system (a container for your microservices)
        /// </summary>
        public static void Create(
            params Func<IMicroservice>[] microservicesFactories)
        {
            if (microservicesFactories.Length == 0)
                throw new ArgumentOutOfRangeException("The user should provide at least one microservice factory");

            lock (_lockObject)
            {
                // Double checking if
                if (multiContainer != null || isRunning)
                    return;

                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
                System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
                System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);


                mainContainer = ResolveCommonDependenciesForMainContainer(UseSignalRLog, Verbose);

                multiContainer = new MultiMicroserviceContainer(
                    mainContainer.Resolve<IBus>(),
                    mainContainer.Resolve<ILogger>(),
                    microservicesFactories.Select(f => f.Invoke()));

                if (OcassionallyConnectedSources != null)
                {
                    var inMemoryEventPublisher = mainContainer.Resolve<IInMemoryEventPublisher>();
                    OcassionallyConnectedSources.ForEach(x => inMemoryEventPublisher.Register(x));
                }

                multiContainer.Start();
                isRunning = true;
            }
        }

        public static IProcessor ResolveProcessor(string name)
        {
            IUnityContainer container;
            if (childContainers.TryGetValue(name, out container))
                return container.Resolve<IProcessor>();

            throw new ArgumentException($"The event processor {name} is not in the event system");
        }

        private static IUnityContainer ResolveCommonDependenciesForMainContainer(bool useSignalRLog, bool verbose)
        {
            var mainContainer = new UnityContainer();

            var log = useSignalRLog ? (ILogger)SignalRLogger.GetResolvedSignalRLogger(verbose) : new ConsoleLogger(verbose);
            mainContainer.RegisterInstance<ILogger>(log);

            // Only one instance of the event publisher sould be in a node.
            mainContainer.RegisterInstance<IInMemoryEventPublisher>(new InMemoryEventPublisher(log));

            var serializer = new JsonTextSerializer();
            mainContainer.RegisterInstance<ITextSerializer>(serializer);

            var time = new UtcTimeProvider() as IUtcTimeProvider;
            mainContainer.RegisterInstance<IUtcTimeProvider>(time);

            mainContainer.RegisterInstance<IGuidProvider>(new SequentialGuid());

            // Do not share this with child dependencies
            mainContainer.RegisterInstance<IBus>(new Bus());

            return mainContainer;
        }

        internal static IUnityContainer ResolveNewChildContainerAndRegisterInMemorySubscriptions(string microserviceName)
        {
            var newContainer = new UnityContainer();
            newContainer.RegisterInstance<ILogger>(mainContainer.Resolve<ILogger>());
            // We resolve the in memory event publisher. There sould be only one instance of it.
            newContainer.RegisterInstance<IInMemoryEventPublisher>(mainContainer.Resolve<IInMemoryEventPublisher>());
            newContainer.RegisterInstance<ITextSerializer>(mainContainer.Resolve<ITextSerializer>());
            newContainer.RegisterInstance<IUtcTimeProvider>(mainContainer.Resolve<IUtcTimeProvider>());
            newContainer.RegisterInstance<IGuidProvider>(mainContainer.Resolve<IGuidProvider>());

            childContainers.Add(microserviceName, newContainer);

            return newContainer;
        }
    }
}
