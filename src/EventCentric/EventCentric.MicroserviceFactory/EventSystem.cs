using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Factory;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Persistence.SqlServer;
using EventCentric.Publishing;
using EventCentric.Publishing.Dto;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;

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

        public void Create(params Func<IMicroservice>[] microservicesFactories)
        {
            EventSystem.Create(microservicesFactories);
        }
    }

    public class EventSystem
    {
        private static IInMemoryEventPublisher mainPublisher;
        private static readonly object _lockObject = new object();
        private static MultiMicroserviceContainer multiContainer = null;
        private static bool isRunning = false;

        internal static bool UseSignalRLog = false;
        internal static bool Verbose = false;

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

                var log = mainContainer.Resolve<ILogger>();
                PrintSystemInfo(log, microservicesFactories.Length);

                multiContainer = new MultiMicroserviceContainer(
                    mainContainer.Resolve<IBus>(),
                    mainContainer.Resolve<ILogger>(),
                    microservicesFactories.Select(f => f.Invoke()));

                multiContainer.Start();

                isRunning = true;
                log.Log($"The Event Centric System is now up and running!");
            }
        }

        public static IProcessor<T> ResolveProcessor<T>(string name) where T : class, IEventSourced
        {
            IUnityContainer container;
            if (childContainers.TryGetValue(name, out container))
                return container.Resolve<IProcessor<T>>();

            throw new ArgumentException($"The event processor {name} is not registered in the event system");
        }

        public static IPollableEventSource ResolveEventSource(string name)
        {
            IUnityContainer container;
            if (childContainers.TryGetValue(name, out container))
                return container.Resolve<IPollableEventSource>();

            throw new ArgumentException($"The source {name} is not registered in the event system");
        }

        public static void AddOcassionallyConnectedSourceOnTheFly(string microserviceName, string sourceName)
        {
            if (!childContainers.ContainsKey(microserviceName))
                throw new KeyNotFoundException($"The microservice {microserviceName} does not exist!");

            childContainers[microserviceName].Resolve<IBus>().Publish(new AddNewSubscriptionOnTheFly(sourceName, Constants.InMemorySusbscriptionUrl, Constants.OcassionallyConnectedSourceToken));
        }

        public static bool TryUpdateServer(string serverName, PollResponse response, out ServerStatus status)
        {
            return mainPublisher.TryUpdateServer(serverName, response, out status);
        }

        private static void PrintSystemInfo(ILogger log, int processorsCount)
        {
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            var processorCount = Environment.ProcessorCount;

            var mo = new System.Management.ManagementObject("Win32_Processor.DeviceID='CPU0'");
            var cpuSpeed = (uint)(mo["CurrentClockSpeed"]);
            mo.Dispose();

            var logLines = new string[6];
#if !DEBUG
            logLines[0] = $"| RELEASE build detected";
#endif
#if DEBUG
            logLines[0] = $"| DEBUG build detected";
#endif
            logLines[1] = string.Format("| Worker threads:     {0}", workerThreads);
            logLines[2] = string.Format("| OSVersion:          {0}", Environment.OSVersion);
            logLines[3] = string.Format("| ProcessorCount:     {0}", processorCount);
            logLines[4] = string.Format("| ClockSpeed:         {0} MHZ", cpuSpeed);
            logLines[5] = $"| MicroserviceCount: {processorsCount}";

            log.Log($"Starting Event Centric System...", logLines);
        }

        private static IUnityContainer ResolveCommonDependenciesForMainContainer(bool useSignalRLog, bool verbose)
        {
            var mainContainer = new UnityContainer();

            var log = useSignalRLog ? (ILogger)SignalRLogger.GetResolvedSignalRLogger(verbose) : new ConsoleLogger(verbose);
            mainContainer.RegisterInstance<ILogger>(log);

            mainPublisher = new InMemoryEventPublisher(log);

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
            newContainer.RegisterInstance<IInMemoryEventPublisher>(mainPublisher);
            newContainer.RegisterInstance<ITextSerializer>(mainContainer.Resolve<ITextSerializer>());
            newContainer.RegisterInstance<IUtcTimeProvider>(mainContainer.Resolve<IUtcTimeProvider>());
            newContainer.RegisterInstance<IGuidProvider>(mainContainer.Resolve<IGuidProvider>());

            childContainers.Add(microserviceName, newContainer);

            return newContainer;
        }
    }
}
