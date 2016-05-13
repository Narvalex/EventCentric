using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Publishing
{
    // This is an ocassionally connected client publisher. The publisher of the client.
    public class OcassionallyConnectedPublisher : PublisherBase, IPollableEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>
    {
        private readonly string clientVersion;
        private readonly string serverUrl;
        private readonly string serverToken;
        private readonly string serverName;
        private long serverEventCollectionVersion = 0;

        public OcassionallyConnectedPublisher(string streamType, IEventStore store, IBus bus, ILogger log, int eventsToPushMaxCount, TimeSpan pollTimeout,
            string clientVersion, string serverUrl, string serverToken, string serverName)
            : base(streamType, store, bus, log, pollTimeout, eventsToPushMaxCount)
        {
            Ensure.Positive(eventsToPushMaxCount, "eventsToPushMaxCount");

            Ensure.NotNullNeitherEmtpyNorWhiteSpace(clientVersion, nameof(clientVersion));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(serverUrl, nameof(serverUrl));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(serverName, nameof(serverName));

            this.clientVersion = clientVersion;
            this.serverUrl = serverUrl;
            this.serverToken = serverToken;
            this.serverName = serverName;
        }

        public string SourceName => this.streamType;

        public void Handle(StopEventPublisher message)
        {
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            base.Start();
        }

        private void SyncWithServer()
        {
            while (this.store.CurrentEventCollectionVersion == 0)
                Thread.Sleep(1);

            while (!base.stopping)
            {
                try
                {
                    var serverStatus = this.TrySync(this.PollEvents(this.serverEventCollectionVersion, this.serverName));
                    this.serverEventCollectionVersion = serverStatus.EventBufferVersion;
                }
                catch (Exception ex)
                {
                    this.log.Error(ex, ex.Message);

                    // for all exception, we wait 10 seconds
                    Thread.Sleep(10000);
                }
            }
        }

        private ServerStatus TrySync(PollResponse payload)
        {
            HttpResponseMessage response;
            using (var client = this.CreateHttpClient())
            {
                response = client.PostAsJsonAsync(this.serverUrl, payload).Result;
            }
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsAsync<ServerStatus>().Result;

            throw new HttpRequestException($"The attempt to make a request to {serverUrl} got a status code of {(int)response.StatusCode}.");
        }

        /// <summary>
        /// More info on credentials: http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
        /// </summary>
        /// <returns></returns>
        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = this.longPollingTimeout;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.serverToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        protected override void OnStarting()
        {
            try
            {
                // We handle exceptions on dao.
                var currentVersion = this.store.CurrentEventCollectionVersion;
                Task.Factory.StartNewLongRunning(() => this.SyncWithServer());

                this.log.Log($"{this.SourceName} publisher started. Current event collection version is: {currentVersion}");

                // Event-sourcing-like approach :)
                this.bus.Publish(new EventPublisherStarted());
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "An error ocurred while starting event publisher.");
                this.bus.Publish(new FatalErrorOcurred(new FatalErrorException("An exception ocurred while starting publisher", ex)));
            }
        }

        protected override void OnStopping()
        {
            this.log.Log($"{this.serverName} publisher stopped");
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
