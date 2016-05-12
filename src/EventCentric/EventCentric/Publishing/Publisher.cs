using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using System;

namespace EventCentric.Publishing
{
    public class Publisher : PublisherBase, IPollableEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<EventStoreHasBeenUpdated>
    {
        // locks
        private readonly object versionlock = new object();

        public Publisher(string streamType, IBus bus, ILogger log, IEventDao dao, int eventsToPushMaxCount, TimeSpan pollTimeout)
            : base(streamType, bus, log, dao, pollTimeout, eventsToPushMaxCount)
        { }

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

        protected override void OnStarting()
        {
            try
            {
                // We handle exceptions on dao.
                var currentVersion = this.dao.GetEventCollectionVersion();

                this.log.Log($"{this.SourceName} publisher started. Current event collection version is {currentVersion}");

                // Event-sourcing-like approach :)
                this.bus.Publish(new EventStoreHasBeenUpdated(currentVersion));
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
            this.log.Log($"{this.SourceName} publisher stopped");
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
