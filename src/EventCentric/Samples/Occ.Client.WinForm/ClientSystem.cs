using EventCentric;
using EventCentric.Config;
using EventCentric.Publishing;
using Microsoft.Practices.Unity;
using Occ.Client.Shared;
using System.Configuration;

namespace Occ.Client.WinForm
{
    public class ClientSystem
    {
        private IUnityContainer container;

        public IUnityContainer Container => this.container;

        public ClientSystem(string userName)
        {
            this.container = new UnityContainer();
            var config = new EventStoreConfig(userName);

            SingleMicroserviceInitializer.Run(container: this.container, useSignalRLog: false,
                microserviceFactory: () =>
                MicroserviceFactory<ItemClient, ItemClientHandler>
                .CreateEventProcessorWithApp<ItemClientApp>(
                    uniqueName: $"{userName}",
                    appUniqueName: $"{userName}App",
                    container: this.container,
                    eventStoreConfig: config,
                    publisherFactory: (streamType, bus, log, dao, eventsToPushMaxCount, pollTimeout)
                        => new OcassionallyConnectedPublisher(streamType, bus, log, dao, eventsToPushMaxCount, pollTimeout, "v1",
                            "http://localhost:51343/client-proxy/upload", "123", "OccServer")));
        }
    }

    public class EventStoreConfig : IEventStoreConfig
    {
        public EventStoreConfig(string connectionName)
        {
            this.ConnectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            this.Token = ConfigurationManager.AppSettings["token"]; ;
        }

        public string ConnectionString { get; }

        public double LongPollingTimeout => 60000;

        public int PushMaxCount => 100;

        public string Token { get; }
    }
}
