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
    public class Publisher<T> : FSM, IEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        private static readonly string _streamType = typeof(T).Name;
        private readonly IEventDao dao;
        private readonly int eventsToPushMaxCount;
        private readonly TimeSpan longPollingTimeout;
        private long eventCollectionVersion = 0;

        // locks
        private readonly object updatelockObject = new object();
        private static readonly object _eventCollectionVersionLock = new object();

        public Publisher(IBus bus, ILogger log, IEventDao dao, int eventsToPushMaxCount, TimeSpan pollTimeout)
            : base(bus, log)
        {
            Ensure.NotNull(dao, "dao");
            Ensure.Positive(eventsToPushMaxCount, "eventsToPushMaxCount");

            this.dao = dao;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
            this.longPollingTimeout = pollTimeout;
        }

        private long EventCollectionVersion
        {
            get
            {
                lock (_eventCollectionVersionLock)
                {
                    return this.eventCollectionVersion;
                }
            }
            set
            {
                lock (_eventCollectionVersionLock)
                {
                    this.eventCollectionVersion = value;
                }
            }
        }

        public void Handle(EventStoreHasBeenUpdated message)
        {
            lock (this.updatelockObject)
            {
                if (message.EventCollectionVersion > this.EventCollectionVersion)
                    this.EventCollectionVersion = message.EventCollectionVersion;
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
        public PollResponse PollEvents(int lastReceivedVersion)
        {
            bool newEventsWereFound = false;
            var newEvents = new List<NewRawEvent>();
            var stopwatch = Stopwatch.StartNew();
            while (!this.stopping && stopwatch.Elapsed < this.longPollingTimeout)
            {
                if (this.EventCollectionVersion <= lastReceivedVersion)
                    Thread.Sleep(100);
                else
                {
                    newEvents = this.dao.FindEvents(lastReceivedVersion, eventsToPushMaxCount);
                    newEventsWereFound = newEvents.Count > 0 ? true : false;

                    this.log.Trace("Pushing {0} event/s", newEvents.Count);
                    break;
                }
            }

            return new PollResponse(false, newEventsWereFound, _streamType, newEvents, lastReceivedVersion, this.EventCollectionVersion);
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
        }
    }
}
