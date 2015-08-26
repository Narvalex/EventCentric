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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Pulling
{
    public class EventPullerPerStream : FSM,
        IMessageHandler<StartEventPollster>,
        IMessageHandler<StopEventPollster>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private readonly ISubscriptionDao dao;
        private readonly IOldHttpPoller poller;
        private readonly ITextSerializer serializer;
        private readonly ISubscriptionInboxWriter writer;

        private ConcurrentBag<SubscribedStream> subscribedStreams;
        private ConcurrentBag<SubscribedSource> subscribedSources;

        public EventPullerPerStream(IBus bus, ISubscriptionDao dao, ISubscriptionInboxWriter writer, IOldHttpPoller poller, ITextSerializer serializer)
            : base(bus)
        {
            Ensure.NotNull(dao, "dao");
            Ensure.NotNull(poller, "poller");
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(writer, "writer");

            this.dao = dao;
            this.poller = poller;
            this.serializer = serializer;
            this.writer = writer;
        }

        public void Handle(StartEventPollster message)
        {
            base.Start();
        }

        public void Handle(StopEventPollster message)
        {
            base.Stop();
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            var subscription = this.subscribedStreams.Where(s => s.StreamId == message.StreamId).Single();
            subscription.TryUpdateVersion(message.StreamVersion);
            subscription.ExitBusy();
        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            this.writer.LogPosisonedMessage(message.StreamType, message.StreamId, message.Exception);

            this.subscribedStreams
                .Where(s => s.StreamId == message.StreamId && s.StreamType == message.StreamType)
                .Single()
                .MarkAsPoisoned();
        }

        protected override void OnStarting()
        {
            this.subscribedStreams = this.dao.GetSubscribedStreamsOrderedByStreamName();
            this.subscribedSources = this.dao.GetSubscribedSources();
            Task.Factory.StartNewLongRunning(
                // This could be a source of troubles....
                () => this.PollNewEvents()
            );

            Task.Factory.StartNewLongRunning(
                // This could be a source of troubles....
                () => this.PollNewStreams()
            );

            // Ensure to start everything;
            this.bus.Publish(new EventPullerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPullerStopped());
        }

        private void PollNewStreams()
        {
            while (!this.stopping)
            {
                // Poll sources
                var pendingSourceSubscriptions = subscribedSources.Where(s => !s.IsBusy);

                if (!pendingSourceSubscriptions.Any())
                    Thread.Sleep(100);
                else
                {
                    foreach (var subscription in pendingSourceSubscriptions)
                    {
                        subscription.EnterBusy();
                        Task.Factory.StartNewLongRunning(() => PollStreams(subscription));
                    }
                }
            }
        }

        private void PollNewEvents()
        {
            while (!this.stopping)
            {
                var pendingStreamSubscriptions = subscribedStreams.Where(s => !s.IsBusy && !s.IsPoisoned);

                // Poll subscriptions
                if (!pendingStreamSubscriptions.Any())
                    Thread.Sleep(100);
                else
                {
                    // Build request.
                    var batchSizeCount = 0;
                    var dtos = new List<PollEventsDto>();
                    foreach (var subscription in pendingStreamSubscriptions)
                    {
                        // Mark subscription as busy
                        batchSizeCount += 1;
                        subscription.EnterBusy();

                        // Group by Event Sources, asuming that stream types lives in the same url
                        var queryForStreamType = dtos.Where(d => d.StreamType == subscription.StreamType);

                        if (queryForStreamType.Any())
                            queryForStreamType.First().Add(subscription.StreamId, subscription.Version);
                        else
                        {
                            var url = this.subscribedSources.Where(s => s.StreamType == subscription.StreamType).Single().Url;
                            dtos.Add(new PollEventsDto(subscription.StreamType, url, subscription.StreamId, subscription.Version));
                        }

                        if (batchSizeCount == 5)
                        {
                            // Send requests async.
                            dtos.ForEach(dto => Task.Factory.StartNewLongRunning(() => PollEvents(dto)));

                            batchSizeCount = 0;
                            dtos = new List<PollEventsDto>();
                        }
                    }

                    // Send requests async for the last ones
                    dtos.ForEach(dto => Task.Factory.StartNewLongRunning(() => PollEvents(dto)));
                }
            }
        }

        private void PollEvents(PollEventsDto dto)
        {
            // Fill the request with the formatter
            var uri = dto.BaseUrlForPolling + "/eventsource/events";

            dto.ProcessedStreams.ForEach(s =>
                uri = $"{uri}/{s.Key.ToString()}/{s.Value.ToString()}");

            // There is 5 (five) slots for Streams
            var freeSlots = 5 - dto.ProcessedStreams.Count;

            for (int i = 0; i < freeSlots; i++)
                uri = $"{uri}/{default(string).EncodedEmptyString()}/0";

            var response = this.poller.PollEvents(uri);

            if (!response.PollingWasSuccessful)
            {
                dto.ProcessedStreams.ForEach(s =>
                this.subscribedStreams
                    .Where(subscribedStream => subscribedStream.StreamType == dto.StreamType && s.Key == subscribedStream.StreamId)
                    .Single()
                    .ExitBusy());

                return;
            }

            response.Events.ForEach(e =>
            {
                if (e.IsNewEvent)
                    try
                    {
                        // Make a retry logic to avoid mark every error to as a poisoned message.
                        this.bus.Publish(new NewIncomingEvent(this.serializer.Deserialize<IEvent>(e.Payload)));
                    }
                    catch (Exception ex)
                    {
                        foreach (var sub in dto.ProcessedStreams)
                            this.bus.Publish(new IncomingEventIsPoisoned(dto.StreamType, sub.Key, new PoisonMessageException("Poisoned message detected in Event Puller.", ex)));
                    }
                else
                    this.subscribedStreams
                                   .Where(s => s.StreamType == e.StreamType && s.StreamId == e.StreamId)
                                   .Single()
                                   .ExitBusy();
            });

        }

        private void PollStreams(SubscribedSource source)
        {
            var response = this.poller.PollStreams($"{source.Url}/eventsource/streams/{source.StreamCollectionVersion}");
            if (response.NewStreamWasFound)
            {
                try
                {
                    this.writer.CreateNewSubscription(source.StreamType, response.NewStreamId.Value, response.UpdatedStreamCollectionVersion.Value);

                    // Updating source stream collection version, hoping reference type works
                    source.TryUpdateStreamCollectionVersion(response.UpdatedStreamCollectionVersion.Value);
                    // Adding a new subscription
                    this.subscribedStreams.Add(new SubscribedStream(source.StreamType, response.NewStreamId.Value, 0, false));
                }
                catch (Exception)
                { }
            }
            this.subscribedSources
                    .Where(s => s.StreamType == source.StreamType)
                    .Single()
                    .ExitBusy();
        }
    }
}
