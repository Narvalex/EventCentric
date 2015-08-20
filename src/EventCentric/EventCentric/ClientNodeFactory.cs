using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.Queueing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;

namespace EventCentric
{
    public class ClientNodeFactory<T>
    {
        public static INode CreateNode(IUnityContainer container, bool setLocalTime = true, bool setSequentialGuid = true)
        {
            var connectionProvider = ConnectionManager.GetConnectionProvider();
            var connectionString = connectionProvider.ConnectionString;

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var node = new ClientNode(bus);

            var queueWriter = new QueueWriter<T>(() => new StreamDbContext(connectionString), serializer, time, guid);
            var eventBus = new EventQueue(bus, queueWriter);

            // Registering in bus
            bus.Register(node, eventBus);

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);

            return node;
        }
    }
}
