using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Polling
{
    public class Poller : MicroserviceWorker, IMonitoredSubscriber,
        IMessageHandler<StartEventPoller>,
        IMessageHandler<StopEventPoller>,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>,
        IMessageHandler<IncomingEventIsPoisoned>
    {
        private string microserviceName;
        private readonly ISubscriptionRepository repository;
        private readonly ILongPoller poller;
        private readonly ITextSerializer serializer;

        /// <summary>
        /// Queue max count for all subscriptons.
        /// </summary>
        private readonly int queueMaxCount;

        /// <summary>
        /// Flush threshold per subscription.
        /// </summary>
        private readonly int eventsToFlushMaxCount;

        private SubscriptionBuffer[] bufferPool;

        private readonly object lockObject = new object();

        public Poller(IBus bus, ILogger log, ISubscriptionRepository repository, ILongPoller poller, ITextSerializer serializer,
            int queueMaxCount, int eventsToFlushMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(poller, nameof(poller));
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(log, "logger");

            Ensure.Positive(queueMaxCount, "queueMaxCount");
            Ensure.Positive(eventsToFlushMaxCount, "eventsToFlushMaxCount");

            this.repository = repository;
            this.poller = poller;
            this.serializer = serializer;
            this.log = log;

            this.queueMaxCount = queueMaxCount;
            this.eventsToFlushMaxCount = eventsToFlushMaxCount;
        }

        private bool TryFill()
        {
            var subscriptonsReadyForPolling = this.bufferPool
                                                  .Where(s => !s.IsPolling && !s.IsPoisoned && s.NewEventsQueue.Count < queueMaxCount)
                                                  .ToArray();

            if (!subscriptonsReadyForPolling.Any())
                // all subscriptions are being polled
                return false;

            foreach (var subscription in subscriptonsReadyForPolling)
            {
                subscription.IsPolling = true;
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(_ =>
                    poller.PollSubscription(subscription.StreamType, subscription.Url, subscription.Token, subscription.CurrentBufferVersion)), null);
            }

            // there are subscriptions that are being polled
            return true;
        }

        private bool TryFlush(SubscriptionBuffer buffer)
        {
            long processorBufferVersion;
            var eventsInQueueCount = buffer.NewEventsQueue.Count();
            //if (eventsInQueueCount < 1 || buffer.EventsInProcessorByEcv.Any(e => !e.Value.WasProcessed))
            //The buffer is empty or there are still events in the processor
            //return false;

            if (eventsInQueueCount < 1)
                return false;

            var eventsToProcess = new List<IEvent>();
            if (buffer.EventsInProcessorByEcv.Any(e => !e.Value.WasProcessed))
            {
                while (!buffer.EventsInProcessorByEcv.IsEmpty)
                {
                    EventInProcessorBucket toBeRemovedIfApplicable;
                    var min = buffer.EventsInProcessorByEcv.Min(x => x.Key);
                    if (buffer.EventsInProcessorByEcv[min].WasProcessed)
                        buffer.EventsInProcessorByEcv.TryRemove(min, out toBeRemovedIfApplicable);
                    else
                        break;
                }

                if (buffer.EventsInProcessorByEcv.IsEmpty)
                    return false;

                processorBufferVersion = buffer.EventsInProcessorByEcv.Min(x => x.Key);

                // there are still some events in the 'bag', try to add a few more
                int freeSlots;
                var eventsInProcessor = buffer.EventsInProcessorByEcv.Where(x => !x.Value.WasProcessed).Count();
                if (eventsToFlushMaxCount > eventsInProcessor)
                    freeSlots = eventsToFlushMaxCount - eventsInProcessor;
                else
                    // there could be events to process, but there are not empty slots...
                    return false;

                for (int i = 0; i < freeSlots; i++)
                {
                    IEvent @event;
                    if (buffer.NewEventsQueue.TryPeek(out @event))
                    {
                        if (!buffer.EventsInProcessorByEcv.Any(x => x.Value.Event.StreamId == @event.StreamId && !x.Value.WasProcessed))
                        {
                            // there is not an event of the same stream that is still in the processor
                            buffer.NewEventsQueue.TryDequeue(out @event);
                            eventsToProcess.Add(@event);
                        }
                    }
                    else
                        break;
                }

                if (eventsToProcess.Count < 1)
                    // no events could be try to process. No op
                    return false;
#if DEBUG
                this.log.Log($"Optimizing event processing for {eventsToProcess.Count} event/s from {buffer.ProducerName}");
#endif
            }
            else
            {
                // if all events where processed, go here
                for (int i = 0; i < eventsToFlushMaxCount; i++)
                {
                    IEvent @event;
                    if (!buffer.NewEventsQueue.TryDequeue(out @event))
                        break;

                    eventsToProcess.Add(@event);
                }

                // the minimun of the queue
                processorBufferVersion = eventsToProcess.Min(e => e.EventCollectionVersion);
                // Reset the bag;
                buffer.EventsInProcessorByEcv.Clear();
            }

            var streams = eventsToProcess.Select(incomingEvent =>
            {
                ((Message)incomingEvent).ProcessorBufferVersion = processorBufferVersion;

                buffer.EventsInProcessorByEcv.TryAdd(incomingEvent.EventCollectionVersion, new EventInProcessorBucket(incomingEvent));

                return incomingEvent;
                //this.bus.Publish(new NewIncomingEvents(incomingEvent));
            })
            .GroupBy(x => x.StreamId, x => x,
                    (key, group) => new
                    {
                        StreamId = key,
                        Events = group
                    });

            foreach (var stream in streams)
                // no need to order by again
                //this.bus.Publish(new NewIncomingEvents(stream.Events.OrderBy(x => x.Version))); 
                this.bus.Publish(new NewIncomingEvents(stream.Events));

            this.log.Trace($"{this.microserviceName} is handling {eventsToProcess.Count} event/s of {buffer.StreamType} queue with {eventsInQueueCount} event/s pulled from {buffer.Url}");

            return true;
        }

        public void Handle(PollResponseWasReceived message)
        {
            var response = message.Response;

            SubscriptionBuffer subscription;
            try
            {
                subscription = this.bufferPool.Where(s => s.StreamType == response.StreamType).Single();
            }
            catch (Exception ex)
            {
                var errorMessage = $@"An error occurred while receiving poll response message. Check if the stream type matches that of the subscribed source. Remote message type that probably was not found is '{response.StreamType}'";
                this.log.Error(ex, errorMessage);

                this.bus.Publish(
                    new FatalErrorOcurred(
                        new FatalErrorException(errorMessage, ex)));

                throw;
            }

            if (response.NewEventsWereFound)
            {
                var orderedEvents =
                    response.IsSerialized

                    // is serialized, then we need to deserialize them
                    ? response
                        .NewRawEvents
                        .Select(e =>
                        {
                            IEvent incomingEvent;

                            try
                            {
                                incomingEvent = this.serializer.Deserialize<IEvent>(e.Payload);
                            }
#if !DEBUG
                            catch (SerializationException)
                            {
#endif
#if DEBUG
                            catch (SerializationException ex)
                            {
                                // Maybe the event source has new events type that we are not aware off.

                                this.log.Error(ex, "An error ocurred while deserializing a message");
                                //this.log.Error($"An error was detected when serializing a message from {buffer.ProducerName} with event collection number of {raw.EventCollectionVersion}. The message will be ignored.");
#endif
                                incomingEvent = new Message();
                            }
                            catch (Exception ex)
                            {
                                var errorMessage = $"An error occurred while trying to deserialize a payload from an incoming event. Check the inner exception for more information. System will shut down.";
                                this.log.Error(ex, errorMessage);

                                this.bus.Publish(
                                    new FatalErrorOcurred(
                                        new FatalErrorException(errorMessage, ex)));

                                throw;
                            }

                            ((Message)incomingEvent).EventCollectionVersion = e.EventCollectionVersion;
                            return incomingEvent;
                        })
                        .OrderBy(e => e.EventCollectionVersion)

                    // Not serialize, we just need to put in order...
                    : response.Events.OrderBy(e => e.EventCollectionVersion);

                subscription.CurrentBufferVersion = orderedEvents.Max(e => e.EventCollectionVersion);

                foreach (var e in orderedEvents)
                {
                    //this.log.Trace("Event: ECVersion {0}", e.EventCollectionVersion);
                    subscription.NewEventsQueue.Enqueue(e);
                }
            }

            if (!response.ErrorDetected)
            {
                subscription.ConsumerVersion = response.ConsumerVersion;
                subscription.ProducerVersion = response.ProducerVersion;
            }

            subscription.IsPolling = false;

            var messageCount = response.IsSerialized ? response.NewRawEvents.Count() : response.Events.Count();
            this.log.Trace(string.Format($"{this.microserviceName} pulled {0} event/s from {1}", messageCount, subscription.StreamType));
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            this.bufferPool
                    .Where(s => s.StreamType == message.StreamType)
                    .Single()
                        .EventsInProcessorByEcv[message.EventCollectionVersion]
                        .MarkEventAsProcessed();
        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            lock (this.lockObject)
            {
                try
                {
                    var poisonedSubscription = this.bufferPool.Where(s => s.StreamType == message.PoisonedEvent.StreamType).Single();
                    if (poisonedSubscription.IsPoisoned == true)
                        return;

                    poisonedSubscription.IsPoisoned = true;

                    this.repository.FlagSubscriptionAsPoisoned(message.PoisonedEvent, message.Exception);

                    this.bus.Publish(
                        new FatalErrorOcurred(
                            new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception)));
                }
                catch (Exception ex)
                {
                    this.bus.Publish(
                        new FatalErrorOcurred(
                            new FatalErrorException($"An error ocurred while attempting to log poisoned message. {ex.Message}",
                                new FatalErrorException("Fatal error: Inconming event is poisoned.", message.Exception))));
                }
            }
        }

        public void Handle(StopEventPoller message)
        {
            base.Stop();
        }

        public void Handle(StartEventPoller message)
        {
            this.microserviceName = message.MicroserviceName;
            base.Start();
        }

        protected override void OnStarting()
        {
            var lines = new List<string>();
            lines.Add($"| Starting {this.microserviceName} poller...");
            this.bufferPool = this.repository.GetSubscriptions();

            lines.Add(string.Format("| Found {0} subscription/s", bufferPool.Count()));

            var subscriptionCount = 0;
            foreach (var subscription in this.bufferPool)
            {
                subscriptionCount += 1;
                lines.Add($"| --> Subscription {subscriptionCount} | Name: {subscription.StreamType} | {subscription.Url} | Buffer version: {subscription.CurrentBufferVersion}");
            }

            Task.Factory.StartNewLongRunning(() => this.KeepTheBufferFull());
            this.DispatchEventsFromBufferPool();

            // Ensure to start everything;
            lines.Add($"| {this.microserviceName} poller started");
            this.log.Log("", lines.ToArray());

            this.bus.Publish(new EventPollerStarted());
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            this.log.Log($"{this.microserviceName} poller stopped");
            this.bus.Publish(new EventPollerStopped());
        }

        private void DispatchEventsFromBufferPool()
        {
            foreach (var buffer in this.bufferPool)
            {
                Task.Factory.StartNewLongRunning(() =>
                {

                    while (!base.stopping)
                    {
                        var isStarving = !this.TryFlush(buffer);

                        if (isStarving)
                            Thread.Sleep(1);
                    }

                });
            }
        }

        private void KeepTheBufferFull()
        {
            while (!base.stopping)
            {
                var isStarving = !this.TryFill();

                if (isStarving)
                    Thread.Sleep(1);
            }
        }

        public IMonitoredSubscription[] GetSubscriptionsMetrics() => this.bufferPool;

        public List<SubscriptionBuffer> GetBufferPool()
        {
            var pool = new List<SubscriptionBuffer>();
            pool.AddRange(this.bufferPool);
            return pool;
        }

        public void StopSilently()
        {
            this.stopping = true;
        }
    }
}
