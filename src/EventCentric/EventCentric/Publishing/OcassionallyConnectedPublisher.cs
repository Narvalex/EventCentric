using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Publishing.Dto;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Publishing
{
    public class OcassionallyConnectedPublisher : MicroserviceWorker, IEventPublisher,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        private readonly string streamType;
        private readonly IEventDao dao;
        private readonly int eventsToPushMaxCount;
        private readonly TimeSpan longPollingTimeout;

        private readonly string clientVersion;
        private readonly string serverUrl;
        private readonly string serverToken;
        private readonly string serverName;
        private long serverEventCollectionVersion = 0;

        // locks
        private readonly object updateVersionlock = new object();
        private long eventCollectionVersion = 0;

        public OcassionallyConnectedPublisher(string streamType, IBus bus, ILogger log, IEventDao dao, int eventsToPushMaxCount, TimeSpan pollTimeout,
            string clientVersion, string serverUrl, string serverToken, string serverName)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, "streamType");
            Ensure.NotNull(dao, "dao");
            Ensure.Positive(eventsToPushMaxCount, "eventsToPushMaxCount");

            Ensure.NotNullNeitherEmtpyNorWhiteSpace(clientVersion, nameof(clientVersion));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(serverUrl, nameof(serverUrl));
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(serverName, nameof(serverName));

            this.streamType = streamType;
            this.dao = dao;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
            this.longPollingTimeout = pollTimeout;

            this.clientVersion = clientVersion;
            this.serverUrl = serverUrl;
            this.serverToken = serverToken;
            this.serverName = serverName;
        }

        public string SourceName => this.streamType;

        public void Handle(EventStoreHasBeenUpdated message)
        {
            lock (this.updateVersionlock)
            {
                if (message.EventCollectionVersion > this.eventCollectionVersion)
                    this.eventCollectionVersion = message.EventCollectionVersion;
            }
        }

        public void Handle(StopEventPublisher message)
        {
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            base.Start();
        }

        /// <remarks>
        /// Timeout implementation inspired by: http://stackoverflow.com/questions/5018921/implement-c-sharp-timeout
        /// </remarks>
        public PollResponse PollEvents(long consumerVersion, string consumerName)
        {
            var newEvents = new List<NewRawEvent>();

            // last received version could be somehow less than 0. I found once that was -1, 
            // and was always pushing "0 events", as the signal r tracing showed (27/10/2015) 
            if (consumerVersion < 0)
                consumerVersion = 0;

            // the consumer says that is more updated than the source. That is an error. Maybe the publisher did not started yet!
            if (this.eventCollectionVersion < consumerVersion)
                return new PollResponse(true, false, this.streamType, newEvents, consumerVersion, this.eventCollectionVersion);

            bool newEventsWereFound = false;
            var stopwatch = Stopwatch.StartNew();
            while (!this.stopping && stopwatch.Elapsed < this.longPollingTimeout)
            {
                if (this.eventCollectionVersion == consumerVersion)
                    // consumer is up to date, and now is waiting until something happens!
                    Thread.Sleep(1);

                // weird error, but is crash proof. Once i had an error where in an infinite loop there was an error saying: Pushing 0 events to....
                // A Charly le paso. Sucede que limpio la base de datos y justo queria entregar un evento y no devolvia nada.
                else if (this.eventCollectionVersion > consumerVersion)
                {
                    newEvents = this.dao.FindEvents(consumerVersion, eventsToPushMaxCount);

                    if (newEvents.Count > 0)
                    {
                        newEventsWereFound = true;
                        this.log.Trace($"Pushing {newEvents.Count} event/s to {consumerName}");
                        break;
                    }
                    else
                    {
                        // Lo que le paso a charly.
                        newEventsWereFound = false;
                        this.log.Trace($"There is an error in the event store. The consumer [{consumerName}] version is {consumerVersion} and the local event collection version should be {this.eventCollectionVersion} but it is not. The event store is currupted.");
                        break;
                    }
                }
                else
                    // bizzare, but helpful to avoid infinite loops
                    break;
            }

            return new PollResponse(false, newEventsWereFound, this.streamType, newEvents, consumerVersion, this.eventCollectionVersion);
        }

        private void SyncWithServer()
        {
            while (this.eventCollectionVersion == 0)
                Thread.Sleep(1);

            while (!base.stopping)
            {
                try
                {
                    var serverStatus = this.TrySync(new ClientData(this.clientVersion, this.PollEvents(this.serverEventCollectionVersion, this.serverName)));
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

        private ServerStatus TrySync(ClientData payload)
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
                var currentVersion = this.dao.GetEventCollectionVersion();
                Task.Factory.StartNewLongRunning(() => this.SyncWithServer());

                this.log.Trace("Current event collection version is: {0}", currentVersion);
                this.log.Trace("Publisher started");
                // Event-sourcing-like approach :)
                this.bus.Publish(
                    new EventStoreHasBeenUpdated(currentVersion),
                    new EventPublisherStarted());
            }
            catch (Exception ex)
            {
                this.log.Error(ex, "An error ocurred while starting event publisher.");
                this.bus.Publish(new FatalErrorOcurred(new FatalErrorException("An exception ocurred while starting publisher", ex)));
            }
        }

        protected override void OnStopping()
        {
            this.log.Trace("Publisher stopped");
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
