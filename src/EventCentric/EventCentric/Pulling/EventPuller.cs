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
        IMessageHandler<IncomingMessageIsPoisoned>,
        IMessageHandler<NewSubscriptionAcquired>
    {
        private readonly ISubscriptionDao dao;
        private readonly IHttpPoller poller;
        private readonly ITextSerializer serializer;

        private ConcurrentBag<Subscription> subscriptions;

        public EventPuller(IBus bus, ISubscriptionDao dao, IHttpPoller poller, ITextSerializer serializer)
            : base(bus)
        {
            Ensure.NotNull(dao, "dao");
            Ensure.NotNull(poller, "poller");
            Ensure.NotNull(serializer, "serializer");

            this.dao = dao;
            this.poller = poller;
            this.serializer = serializer;
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
            var subscription = this.subscriptions.Where(s => s.StreamId == message.StreamId).Single();
            subscription.UpdateVersion(message.StreamVersion);
            subscription.ExitBusy();
        }

        public void Handle(NewSubscriptionAcquired message)
        {
            var url = this.subscriptions
                          .Where(s => message.StreamId == s.StreamId && message.StreamType == s.StreamType)
                          .Single()
                          .Url;

            var newSubscription = new Subscription(message.StreamType, message.StreamId, url, 0, false);

            this.subscriptions.Add(newSubscription);
        }

        public void Handle(IncomingMessageIsPoisoned message)
        {
            this.subscriptions
                .Where(s => s.StreamId == message.StreamId && s.StreamType == message.StreamType)
                .Single()
                .MarkAsPoisoned();
        }

        protected override void OnStarting()
        {
            this.subscriptions = this.dao.GetSubscriptionsOrderedByStreamName();
            Task.Factory.StartNewLongRunning(() => this.CountinuoslyPullEvents());

            // Ensure to start everything;
            this.bus.Publish(new EventPullerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.bus.Publish(new EventPullerStopped());
        }

        private void CountinuoslyPullEvents()
        {
            while (!this.stopping)
            {
                var pendingSubscriptions = subscriptions.Where(s => !s.IsBusy && !s.IsPoisoned);

                if (pendingSubscriptions.Count() == 0)
                    Thread.Sleep(100);
                else
                {
                    // Build request.
                    var batchSizeCount = 0;
                    var dtos = new List<PollRemoteEndpointDto>();
                    foreach (var subscription in pendingSubscriptions)
                    {
                        // Mark subscription as busy
                        batchSizeCount += 1;
                        subscription.EnterBusy();

                        // Group by Event Sources, asuming that stream types lives in the same url
                        var queryForStreamType = dtos.Where(d => d.StreamType == subscription.StreamType);

                        if (queryForStreamType.Any())
                            queryForStreamType.First().Add(subscription.StreamId, subscription.Version);
                        else
                            dtos.Add(new PollRemoteEndpointDto(subscription.StreamType, subscription.Url, subscription.StreamId, subscription.Version));

                        if (batchSizeCount == 5)
                        {
                            // Send requests async.
                            dtos.ForEach(dto => Task.Factory.StartNewLongRunning(() => PollEvents(dto)));

                            batchSizeCount = 0;
                            dtos = new List<PollRemoteEndpointDto>();
                        }
                    }

                    // Send requests async for the last ones
                    dtos.ForEach(dto => Task.Factory.StartNewLongRunning(() => PollEvents(dto)));
                }
            }
        }

        private void PollEvents(PollRemoteEndpointDto dto)
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
                var response = this.poller.Poll(uri);

                response.Events.ForEach(e =>
                {
                    if (e.IsNewEvent)
                        this.bus.Publish(new NewIncomingEvent(this.serializer.Deserialize<IEvent>(e.Payload)));
                    else
                        this.subscriptions
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
    }
}
