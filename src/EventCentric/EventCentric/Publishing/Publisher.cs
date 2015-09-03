using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
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
        private readonly int eventsToPushMaxCount = 100;
        private readonly int attemptsMaxCount = 300;
        private readonly object lockObject = new object();

        private volatile int eventCollectionVersion = 0;

        public Publisher(IBus bus, ILogger log, IEventDao dao, int eventsToPushMaxCount, int attemptsMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(dao, "dao");
            Ensure.Positive(eventsToPushMaxCount, "eventsToPushMaxCount");
            Ensure.Positive(attemptsMaxCount, "attemptsMaxCount");

            this.dao = dao;
            this.eventsToPushMaxCount = eventsToPushMaxCount;
            this.attemptsMaxCount = attemptsMaxCount;
        }

        public void Handle(EventStoreHasBeenUpdated message)
        {
            lock (this.lockObject)
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

        public PollResponse PollEvents(int lastReceivedVersion)
        {
            bool newEventsWereFound = false;
            var newEvents = new List<NewRawEvent>();
            var attemps = 0;
            while (!this.stopping && attemps <= attemptsMaxCount)
            {
                attemps += 1;
                if (this.eventCollectionVersion <= lastReceivedVersion)
                    Thread.Sleep(100);
                else
                {
                    newEvents = this.dao.FindEvents(lastReceivedVersion, eventsToPushMaxCount);
                    newEventsWereFound = newEvents.Count > 0 ? true : false;

#if DEBUG
                    this.log.Trace("Pushing {0} events", newEvents.Count);
#endif
                    break;
                }
            }

            return new PollResponse(newEventsWereFound, _streamType, newEvents);
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
