using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EventCentric.Publishing
{
    public class EventPublisher<T> : FSM, IEventSource,
        IMessageHandler<StartEventPublisher>,
        IMessageHandler<StopEventPublisher>,
        IMessageHandler<StreamHasBeenUpdated>
            where T : class, IEventSourced
    {
        private readonly static string _streamType = typeof(T).Name;
        private readonly IStreamDao dao;

        private readonly ITextSerializer serializer;

        private ConcurrentDictionary<Guid, int> streamVersionsById;

        public EventPublisher(IBus bus, IStreamDao dao, ITextSerializer serializer)
            : base(bus)
        {
            Ensure.NotNull(dao, "dao");
            Ensure.NotNull(serializer, "serializer");

            this.dao = dao;
            this.serializer = serializer;
        }

        public void Handle(StreamHasBeenUpdated message)
        {
            this.streamVersionsById.AddOrUpdate(
                key: message.StreamId,
                addValue: message.UpdatedStreamVersion,
                updateValueFactory: (streamId, currentVersion) => message.UpdatedStreamVersion > currentVersion ? message.UpdatedStreamVersion : currentVersion);
        }

        public void Handle(StopEventPublisher message)
        {
            base.Stop();
        }

        public void Handle(StartEventPublisher message)
        {
            base.Start();
        }

        public PollResponse PollEvents(PollRequest request)
        {
            var responseList = new List<PolledEventData>(5);
            request.StreamVersionsFromSubscriber.ForEach(pollerVersion =>
            {
                var currentVersion = this.streamVersionsById.TryGetValue(pollerVersion.Key);
                // Check if version is still up to date
                if (pollerVersion.Value >= currentVersion)
                    responseList.Add(new PolledEventData(_streamType, pollerVersion.Key, false, string.Empty));
                else
                // If poller's version is stale, pull from event store the next one
                {
                    string payload = this.dao.GetNextEventPayload(pollerVersion.Key, pollerVersion.Value);
                    responseList.Add(new PolledEventData(_streamType, pollerVersion.Key, true, payload));
                }
            });

            return new PollResponse(responseList);
        }

        protected override void OnStarting()
        {
            this.streamVersionsById = this.dao.GetStreamsVersionsById();
            this.bus.Publish(new EventPublisherStarted());
        }

        protected override void OnStopping()
        {
            this.bus.Publish(new EventPublisherStopped());
        }
    }
}
