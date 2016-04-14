using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Polling;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EventCentric.Publishing
{
    public class Publisher : MicroserviceWorker, IPollableEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        private readonly string streamType;
        private readonly IEventDao dao;
        private readonly int eventsToPushMaxCount;
        private readonly TimeSpan longPollingTimeout;

        // locks
        private readonly object versionlock = new object();
        private long eventCollectionVersion = 0;

        public Publisher(string streamType, IBus bus, ILogger log, IEventDao dao, int eventsToPushMaxCount, TimeSpan pollTimeout)
            : base(bus, log)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, "streamType");
            Ensure.NotNull(dao, "dao");
            Ensure.Positive(eventsToPushMaxCount, "eventsToPushMaxCount");

            this.streamType = streamType;
            this.dao = dao;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
            this.longPollingTimeout = pollTimeout;
        }

        public string SourceName => this.streamType;

        public void Handle(EventStoreHasBeenUpdated message)
        {
            lock (this.versionlock)
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
            var ecv = this.eventCollectionVersion;

            var newEvents = new List<NewRawEvent>();

            // last received version could be somehow less than 0. I found once that was -1, 
            // and was always pushing "0 events", as the signal r tracing showed (27/10/2015) 
            if (consumerVersion < 0)
                consumerVersion = 0;

            // the consumer says that is more updated than the source. That is an error. Maybe the publisher did not started yet!
            if (ecv < consumerVersion)
                return new PollResponse(true, false, this.streamType, newEvents, consumerVersion, ecv);

            bool newEventsWereFound = false;
            var stopwatch = Stopwatch.StartNew();
            while (!this.stopping && stopwatch.Elapsed < this.longPollingTimeout)
            {
                if (ecv == consumerVersion)
                {
                    // consumer is up to date, and now is waiting until something happens!
                    Thread.Sleep(1);
                    ecv = this.eventCollectionVersion; // I Forgot to update!
                }
                // weird error, but is crash proof. Once i had an error where in an infinite loop there was an error saying: Pushing 0 events to....
                // A Charly le paso. Sucede que limpio la base de datos y justo queria entregar un evento y no devolvia nada.
                else if (ecv > consumerVersion)
                {
                    newEvents = this.dao.FindEvents(consumerVersion, eventsToPushMaxCount);

                    if (newEvents.Count > 0)
                    {
                        newEventsWereFound = true;
                        this.log.Trace($"{this.SourceName} publisher is pushing {newEvents.Count} event/s to {consumerName}");
                        break;
                    }
                    else
                    {
                        // Lo que le paso a charly.
                        newEventsWereFound = false;
                        //this.log.Error($"There is an error in the event store or a racy condition. The consumer [{consumerName}] version is {consumerVersion} and the local event collection version should be {this.eventCollectionVersion} but it is not.");
                        this.bus.Publish(new FatalErrorOcurred(new FatalErrorException($"There is an error in the event store or a racy condition. The consumer [{consumerName}] version is {consumerVersion} and the local event collection version should be {this.eventCollectionVersion} but it is not.")));
                        break;
                    }
                }
                else
                    // bizzare, but helpful to avoid infinite loops
                    break;
            }

            return new PollResponse(false, newEventsWereFound, this.streamType, newEvents, consumerVersion, ecv);
        }

        protected override void OnStarting()
        {
            try
            {
                // We handle exceptions on dao.
                var currentVersion = this.dao.GetEventCollectionVersion();

                this.log.Log($"{this.SourceName} publisher started. Current event collection version is {currentVersion}");

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
            this.log.Log($"{this.SourceName} publisher stopped");
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
