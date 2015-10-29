using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EventCentric.Publishing
{
    public class Publisher : FSM, IEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        private readonly string streamType;
        private readonly IEventDao dao;
        private readonly int eventsToPushMaxCount;
        private readonly TimeSpan longPollingTimeout;

        // locks
        private readonly object updateVersionlock = new object();
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
            this.log.Trace("Stopping publisher");
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            this.log.Trace("Starting publisher");
            base.Start();
        }

        /// <remarks>
        /// Timeout implementation inspired by: http://stackoverflow.com/questions/5018921/implement-c-sharp-timeout
        /// </remarks>
        public PollResponse PollEvents(long lastReceivedVersion, string consumerName)
        {
            var newEvents = new List<NewRawEvent>();

            // last received version could be somehow less than 0. I found once that was -1, 
            // and was always pushing "0 events", as the signal r tracing showed (27/10/2015) 
            if (lastReceivedVersion < 0)
                lastReceivedVersion = 0;

            // the consumer says that is more updated than the source. That is an error
            if (this.eventCollectionVersion < lastReceivedVersion)
                return new PollResponse(true, false, this.streamType, newEvents, lastReceivedVersion, this.eventCollectionVersion);

            bool newEventsWereFound = false;
            var stopwatch = Stopwatch.StartNew();
            while (!this.stopping && stopwatch.Elapsed < this.longPollingTimeout)
            {
                if (this.eventCollectionVersion == lastReceivedVersion)
                    // consumer is up to date, and now is waiting until something happens!
                    Thread.Sleep(100);
                else
                {
                    newEvents = this.dao.FindEvents(lastReceivedVersion, eventsToPushMaxCount);
                    newEventsWereFound = newEvents.Count > 0 ? true : false;

                    this.log.Trace($"Pushing {newEvents.Count} event/s to {consumerName}");
                    break;
                }
            }

            return new PollResponse(false, newEventsWereFound, this.streamType, newEvents, lastReceivedVersion, this.eventCollectionVersion);
        }

        protected override void OnStarting()
        {
            try
            {
                // We handle exceptions on dao.
                var currentVersion = this.dao.GetEventCollectionVersion();

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
            this.bus.Publish(new EventPublisherStopped());
            this.log.Trace("Event publishers stopped");
        }
    }
}
