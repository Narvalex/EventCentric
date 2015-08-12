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
    public class EventPuller : Worker,
        IMessageHandler<StartEventPuller>,
        IMessageHandler<StopEventPuller>
    {
        private readonly ISubscriptionsDao dao;
        private readonly Func<IHttpClient> httpClientFactory;
        private readonly ITextSerializer serializer;

        private ConcurrentBag<Subscription> subscriptions;

        public EventPuller(IBus bus, ISubscriptionsDao dao, Func<IHttpClient> httpClientFactory, ITextSerializer serializer)
            : base(bus)
        {
            this.dao = dao;
            this.httpClientFactory = httpClientFactory;
            this.serializer = serializer;
        }

        public void Handle(StopEventPuller message)
        {
            base.Stop();
        }

        public void Handle(StartEventPuller message)
        {
            base.Start();
        }

        protected override void OnStarting()
        {
            this.subscriptions = new ConcurrentBag<Subscription>(this.dao.GetSubscriptionsOrderedByStreamName());
            Task.Factory.StartNewLongRunning(() => this.CountinuoslyPullEvents());

            // Ensure to start everything;
            this.bus.Publish(new EventPullerStarted());
        }

        protected override void OnStopping()
        {
            this.subscriptions = null;

            // Ensure to stop everything;
            this.bus.Publish(new EventPullerStopped());
        }

        public void CountinuoslyPullEvents()
        {
            while (!this.stop)
            {
                var pendingSubscriptions = subscriptions.Where(s => !s.IsBusy && !s.IsPoisoned);

                if (pendingSubscriptions.Count() == 0)
                    Thread.Sleep(100);
                else
                {
                    // Build request.
                    var batchSizeCount = 0;
                    var dtos = new List<PollEventsDto>();
                    foreach (var subscription in pendingSubscriptions)
                    {
                        // Mark subscription as busy
                        batchSizeCount += 1;
                        subscription.IsBusy = true;

                        // Group by streamType, asuming that stream types lives in the same url
                        var queryForStreamType = dtos.Where(d => d.StreamType == subscription.StreamType);
                        if (queryForStreamType.Any())
                            queryForStreamType.First().Add(subscription.StreamId, subscription.Version);
                        else
                            dtos.Add(new PollEventsDto(subscription.StreamType, subscription.Url, subscription.StreamId, subscription.Version));

                        if (batchSizeCount == 5)
                        {
                            // Send requests async.
                            dtos.ForEach(dto => Task.Factory.StartNewLongRunning(() => PollEvents(dto)));

                            batchSizeCount = 0;
                            dtos = new List<PollEventsDto>();
                        }
                    }
                }
            }
        }

        private void PollEvents(PollEventsDto dto)
        {
            var serializedResponse = string.Empty;
            using (var http = this.httpClientFactory.Invoke())
            {
                // Fill the request with the formatter
                var uri = string.Empty;
                serializedResponse = http.GetStringAsync(uri).Result;
            }

            var response = this.serializer.Deserialize<PollResponse>(serializedResponse);

            response.Events.ForEach(e =>
            {
                if (e.IsNewEvent)


            });

            // If failure or no new event, the subscription is not busy anymore
            var failedId = Guid.Empty;
            this.subscriptions.Where(s => s.StreamId == failedId).First().IsBusy = false;

            // If success, then we publish to the bus that a new message was found;
            // The processor will notify that the message was processed an will update the version, or notify that is poisoned after
            //  a few retries. 
        }
    }
}
