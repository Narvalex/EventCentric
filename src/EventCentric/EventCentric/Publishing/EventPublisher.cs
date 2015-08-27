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
        IMessageHandler<StreamHasBeenUpdated>
    {
        //private ConcurrentDictionary<Guid, int> streamVersionsById;
        //private volatile int streamCollectionVersion;

        // New
        private static readonly string _streamType = typeof(T).Name;
        private readonly IEventDao dao;
        private const int responseLength = 50;

        private volatile int eventCollectionVersion;

        public EventPublisher(IBus bus, IEventDao dao)
            : base(bus)
        {
            Ensure.NotNull(dao, "dao");

            this.dao = dao;
        }

        public void Handle(StreamHasBeenUpdated message)
        {
            //this.streamVersionsById.AddOrUpdate(
            //    key: message.StreamId,
            //    addValue: message.UpdatedStreamVersion,
            //    updateValueFactory: (streamId, currentVersion) => message.UpdatedStreamVersion > currentVersion ? message.UpdatedStreamVersion : currentVersion);

            //this.streamCollectionVersion = message.UpdatedStreamCollectionVersion > this.streamCollectionVersion ? message.UpdatedStreamCollectionVersion : this.streamCollectionVersion;
        }

        public void Handle(StopEventPublisher message)
        {
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            base.Start();
        }

        public PollResponse PollEvents(int eventBufferVersion)
        {
            bool newEventsFound = false;
            var newEvents = new List<NewEvent>();
            var attemps = 0;
            while (!this.stopping && attemps <= 100)
            {
                attemps += 1;
                if (this.eventCollectionVersion <= eventBufferVersion)
                    Thread.Sleep(100);
                else
                {
                    newEvents = this.dao.GetEvents(eventBufferVersion, responseLength);
                    break;
                }
            }

            return new PollResponse(newEventsFound, _streamType, newEvents);
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
