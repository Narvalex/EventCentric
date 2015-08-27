using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.Publishing;
using EventCentric.Queueing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System.Data.Entity;

namespace EventCentric
{
    public class ClientNodeFactory<T>
    {
        public static INode CreateNode(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var connectionProvider = ConnectionManager.GetConnectionProvider();
            var connectionString = connectionProvider.ConnectionString;

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var node = new ClientNode(bus);

            var queueWriter = new QueueWriter<T>(() => new EventQueueDbContext(connectionString), serializer, time, guid);
            var eventDao = new EventDao(() => new EventQueueDbContext(connectionString));
            var eventBus = new EventQueue(bus, queueWriter);
            var eventPublisher = new EventPublisher<T>(bus, eventDao);

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);

            return node;
        }
    }
}
