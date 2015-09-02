using EventCentric.Database;
using EventCentric.Messaging;
using EventCentric.NodeFactory.Log;
using EventCentric.Publishing;
using EventCentric.Queueing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Utils;
using Microsoft.Practices.Unity;
using System;
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

            Func<bool, EventQueueDbContext> eventQueueDbContextFactory = isReadOnly => new EventQueueDbContext(isReadOnly, connectionString);

            var serializer = new JsonTextSerializer();
            var time = setLocalTime ? new LocalTimeProvider() as ITimeProvider : new UtcTimeProvider() as ITimeProvider;
            var guid = setSequentialGuid ? new SequentialGuid() as IGuidProvider : new DefaultGuidProvider() as IGuidProvider;

            var bus = new Bus();
            var log = Logger.ResolvedLogger;

            var node = new ClientNode(bus, log);

            var queueWriter = new QueueWriter<T>(eventQueueDbContextFactory, serializer, time, guid);
            var eventDao = new EventDao(eventQueueDbContextFactory);
            var eventBus = new EventQueue(bus, log, queueWriter);
            var eventPublisher = new EventPublisher<T>(bus, log, eventDao);

            // Register for DI
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventSource>(eventPublisher);

            return node;
        }
    }
}
