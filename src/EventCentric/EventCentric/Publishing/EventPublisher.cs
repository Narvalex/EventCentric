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

        public OldPollEventsResponse PollEvents(PollEventsRequest request)
        {
            var responseList = new List<OldPolledEventData>(5);

            //request.StreamVersionsFromSubscriber.ForEach(pollerVersion =>
            //{
            //    var currentVersion = this.streamVersionsById.TryGetValue(pollerVersion.Key);
            //    // Check if version is still up to date
            //    if (pollerVersion.Value >= currentVersion)
            //        responseList.Add(new OldPolledEventData(_streamType, pollerVersion.Key, false, string.Empty));
            //    else
            //    // If poller's version is stale, pull from event store the next one
            //    {
            //        string payload = this.dao.GetNextEventPayload(pollerVersion.Key, pollerVersion.Value);
            //        responseList.Add(new OldPolledEventData(_streamType, pollerVersion.Key, true, payload));
            //    }
            //});

            return new OldPollEventsResponse(true, responseList);
        }

        public PollResponse PollEvents(int clientVersion)
        {
            bool newEventsFound = false;
            var newEvents = new List<NewEvent>();
            var attemps = 0;
            while (!this.stopping && attemps <= 100)
            {
                attemps += 1;
                var events = this.dao.FindEvents(clientVersion, responseLength);
                if (events != null)
                {
                    newEventsFound = true;
                    newEvents = events;
                    break;
                }
                else
                    Thread.Sleep(100);
            }

            return new PollResponse(newEventsFound, _streamType, newEvents);
        }

        public PollStreamsResponse PollStreams(PollStreamsRequest request)
        {
            PollStreamsResponse response = null;
            //if (this.streamCollectionVersion > request.StreamCollectionVersion)
            //{
            //    var newStream = this.dao.GetNextStreamIdAndStreamCollectionVersion(request.StreamCollectionVersion);
            //    response = new PollStreamsResponse(true, newStream.Item1, newStream.Item2);
            //}
            //else
            //    response = new PollStreamsResponse(false, null, null);

            return response;
        }

        protected override void OnStarting()
        {
            //this.streamVersionsById = this.dao.GetStreamsVersionsById();
            //this.streamCollectionVersion = this.dao.GetStreamCollectionVersion();
            this.bus.Publish(new EventPublisherStarted());
        }

        protected override void OnStopping()
        {
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
