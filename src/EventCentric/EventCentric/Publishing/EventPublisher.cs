using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Transport;
using EventCentric.Utils;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric.Publishing
{
    public class EventPublisher<T> : FSM, IEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        private static readonly string _streamType = typeof(T).Name;
        private readonly IEventDao dao;
        private const int eventsToPushMaxCount = 100;
        private const int attempsMaxCount = 300;
        private readonly object lockObject = new object();

        private volatile int eventCollectionVersion;

        public EventPublisher(IBus bus, IEventDao dao)
            : base(bus)
        {
            Ensure.NotNull(dao, "dao");

            this.dao = dao;
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
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            base.Start();
        }

        public PollResponse PollEvents(int lastReceivedVersion)
        {
            bool newEventsWereFound = false;
            var newEvents = new List<NewRawEvent>();
            var attemps = 0;
            while (!this.stopping && attemps <= attempsMaxCount)
            {
                attemps += 1;
                if (this.eventCollectionVersion <= lastReceivedVersion)
                    Thread.Sleep(100);
                else
                {
                    newEvents = this.dao.FindEvents(lastReceivedVersion, eventsToPushMaxCount);
                    newEventsWereFound = newEvents.Count > 0 ? true : false;
                    break;
                }
            }

            return new PollResponse(newEventsWereFound, _streamType, newEvents);
        }

        protected override void OnStarting()
        {
            this.eventCollectionVersion = this.dao.GetEventCollectionVersion();
            this.bus.Publish(new EventPublisherStarted());
        }

        protected override void OnStopping()
        {
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
