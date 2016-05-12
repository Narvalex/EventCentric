using EventCentric.Factory;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Persistence.SqlServer;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;

namespace EventCentric.MicroserviceFactory
{
    public class ContainerFactory
    {
        internal static IUnityContainer ResolveCommonDependenciesForMainContainer(IUnityContainer container, bool useSignalRLog, bool verbose)
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            System.Data.Entity.Database.SetInitializer<EventQueueDbContext>(null);

            var log = useSignalRLog ? (ILogger)SignalRLogger.GetResolvedSignalRLogger(verbose) : new ConsoleLogger(verbose);
            container.RegisterInstance<ILogger>(log);

            // Only one instance of the event publisher sould be in a node.
            container.RegisterInstance<IInMemoryEventPublisher>(new InMemoryEventPublisher(log));

            var serializer = new JsonTextSerializer();
            container.RegisterInstance<ITextSerializer>(serializer);

            var time = new UtcTimeProvider() as IUtcTimeProvider;
            container.RegisterInstance<IUtcTimeProvider>(time);

            container.RegisterInstance<IGuidProvider>(new SequentialGuid());

            // Do not share this with child dependencies
            container.RegisterInstance<ISystemBus>(new SystemBus());

            return container;
        }

        public static IUnityContainer ResolveDependenciesForNewChildContainer(IUnityContainer mainContainer)
        {
            var newContainer = new UnityContainer();
            newContainer.RegisterInstance<ILogger>(mainContainer.Resolve<ILogger>());
            // We resolve the in memory event publisher. There sould be only one instance of it.
            newContainer.RegisterInstance<IInMemoryEventPublisher>(mainContainer.Resolve<IInMemoryEventPublisher>());
            newContainer.RegisterInstance<ITextSerializer>(mainContainer.Resolve<ITextSerializer>());
            newContainer.RegisterInstance<IUtcTimeProvider>(mainContainer.Resolve<IUtcTimeProvider>());
            newContainer.RegisterInstance<IGuidProvider>(mainContainer.Resolve<IGuidProvider>());
            return newContainer;
        }
    }
}
