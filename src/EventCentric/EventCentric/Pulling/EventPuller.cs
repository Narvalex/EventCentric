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
    public class EventPuller : FSM,
        IMessageHandler<StartEventPuller>,
        IMessageHandler<StopEventPuller>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingMessageIsPoisoned>
    {
        private readonly ISubscriptionDao dao;
        private readonly IHttpPoller poller;
        private readonly ITextSerializer serializer;
        private readonly ISubscriptionInboxWriter writer;

        private ConcurrentBag<SubscribedStream> subscribedStreams;
        private ConcurrentBag<SubscribedSource> subscribedSources;

        public EventPuller(IBus bus, ISubscriptionDao dao, ISubscriptionInboxWriter writer, IHttpPoller poller, ITextSerializer serializer)
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

        public void Handle(StartEventPuller message)
        {
            base.Start();
        }

        public void Handle(StopEventPuller message)
        {
            base.Stop();
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            var subscription = this.subscribedStreams.Where(s => s.StreamId == message.StreamId).Single();
            subscription.TryUpdateVersion(message.StreamVersion);
            subscription.ExitBusy();
        }

        public void Handle(IncomingMessageIsPoisoned message)
        {
            this.subscribedStreams
                .Where(s => s.StreamId == message.StreamId && s.StreamType == message.StreamType)
                .Single()
                .MarkAsPoisoned();
        }

        protected override void OnStarting()
        {
            this.subscribedStreams = this.dao.GetSubscribedStreamsOrderedByStreamName();
            this.subscribedSources = this.dao.GetSubscribedSources();
            Task.Factory.StartNewLongRunning(() => this.PollEventSources());

            // Ensure to start everything;
            this.bus.Publish(new EventPullerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPullerStopped());
        }

        private void PollEventSources()
        {
            while (!this.stopping)
            {
                var pollerIsbusy = new Tuple<bool, bool>(false, false);

                var pendingStreamSubscriptions = subscribedStreams.Where(s => !s.IsBusy && !s.IsPoisoned);

                // Poll subscriptions
                if (!pendingStreamSubscriptions.Any())
                    pollerIsbusy = new Tuple<bool, bool>(true, pollerIsbusy.Item2);
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

                // Poll sources
                var pendingSourceSubscriptions = subscribedSources.Where(s => !s.IsBusy);

                if (!pendingSourceSubscriptions.Any())
                    pollerIsbusy = new Tuple<bool, bool>(pollerIsbusy.Item1, true);
                else
                {
                    foreach (var subscription in pendingSourceSubscriptions)
                    {
                        subscription.EnterBusy();
                        Task.Factory.StartNewLongRunning(() => PollStreams(subscription));
                    }
                }

                if (pollerIsbusy.Item1 && pollerIsbusy.Item2)
                    Thread.Sleep(100);
            }
        }

        private void PollEvents(PollEventsDto dto)
        {
            // Fill the request with the formatter
            var uri = dto.Url;

            dto.ProcessedStreams.ForEach(s =>
                uri = $"{uri}/{s.Key.ToString()}/{s.Value.ToString()}");

            // There is 5 (five) slots for Streams
            var freeSlots = 5 - dto.ProcessedStreams.Count;

            for (int i = 0; i < freeSlots; i++)
                uri = $"{uri}/{default(string).EncodedEmptyString()}/0";

            try
            {
                var response = this.poller.PollEvents(uri);

                response.Events.ForEach(e =>
                {
                    if (e.IsNewEvent)
                        this.bus.Publish(new NewIncomingEvent(this.serializer.Deserialize<IEvent>(e.Payload)));
                    else
                        this.subscribedStreams
                            .Where(s => s.StreamType == e.StreamType && s.StreamId == e.StreamId)
                            .Single()
                            .ExitBusy();
                });
            }
            catch (Exception ex)
            {
                foreach (var sub in dto.ProcessedStreams)
                    this.bus.Publish(new IncomingMessageIsPoisoned(dto.StreamType, sub.Key, new PoisonMessageException("Poisoned message detected in Event Puller.", ex)));
                return;
            }
        }

        private void PollStreams(SubscribedSource source)
        {
            var response = this.poller.PollStreams($"{source.Url}/{source.StreamCollectionVersion}");
            if (response.NewStreamWasFound)
            {
                this.writer.CreateNewSubscription(source.StreamType, response.NewStreamId.Value, response.UpdatedStreamCollectionVersion.Value);

                // Updating source stream collection version, hoping reference type works
                source.TryUpdateStreamCollectionVersion(response.UpdatedStreamCollectionVersion.Value);
                // Adding a new subscription
                this.subscribedStreams.Add(new SubscribedStream(source.StreamType, response.NewStreamId.Value, 0, false));
            }
            else
                this.subscribedSources
                    .Where(s => s.StreamType == source.StreamType)
                    .Single()
                    .ExitBusy();
        }
    }
}
