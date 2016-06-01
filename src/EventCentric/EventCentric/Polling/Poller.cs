using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Publishing;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace EventCentric.Polling
{
    public class Poller : MicroserviceWorker, IMonitoredSubscriber,
        ISystemHandler<StartEventPoller>,
        ISystemHandler<StopEventPoller>,
        ISystemHandler<PollResponseWasReceived>,
        ISystemHandler<IncomingEventHasBeenProcessed>,
        ISystemHandler<IncomingEventIsPoisoned>,
        ISystemHandler<AddNewSubscriptionOnTheFly>
    {
        private bool stopSilently = false;
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
        private Thread thread;

        private ConcurrentBag<SubscriptionBuffer> onTheFlyBufferPool;
        private Thread onTheFlyThread;
        private bool onTheFlySubscriptionsDetected = false;
        private IInMemoryEventPublisherRegistry mainPublisherRegistry;

        private readonly object lockObjectForOnTheFlySub = new object();
        private readonly object lockObjectForPoisonedSubs = new object();

        public Poller(IBus bus, ILogger log, IInMemoryEventPublisherRegistry mainPublisherRegistry, ISubscriptionRepository repository, ILongPoller poller, ITextSerializer serializer,
            int queueMaxCount, int eventsToFlushMaxCount)
            : base(bus, log)
        {
            Ensure.NotNull(repository, "repository");
            Ensure.NotNull(poller, nameof(poller));
            Ensure.NotNull(serializer, "serializer");
            Ensure.NotNull(log, "logger");
            Ensure.NotNull(mainPublisherRegistry, nameof(mainPublisherRegistry));

            Ensure.Positive(queueMaxCount, "queueMaxCount");
            Ensure.Positive(eventsToFlushMaxCount, "eventsToFlushMaxCount");

            this.repository = repository;
            this.poller = poller;
            this.serializer = serializer;
            this.log = log;
            this.mainPublisherRegistry = mainPublisherRegistry;

            this.queueMaxCount = queueMaxCount;
            this.eventsToFlushMaxCount = eventsToFlushMaxCount;
        }

        private bool TryFill(SubscriptionBuffer buffer)
        {
            if (!buffer.IsPolling && !buffer.IsPoisoned && buffer.NewEventsQueue.Count < queueMaxCount)
            {
                buffer.IsPolling = true;
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(_ =>
                    poller.PollSubscription(buffer.StreamType, buffer.Url, buffer.Token, buffer.CurrentBufferVersion)), null);
                return true;
            }

            return false;
        }

        private bool TryFlush(SubscriptionBuffer buffer)
        {

            //var eventsInQueueCount = buffer.NewEventsQueue.Count();
            //if (eventsInQueueCount < 1 || buffer.EventsInProcessorByEcv.Any(e => !e.Value.WasProcessed))
            //The buffer is empty or there are still events in the processor
            //return false;

            if (buffer.NewEventsQueue.IsEmpty)
                return false;

            long processorBufferVersion;
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
                this.log.Trace($"Optimizing event processing for {eventsToProcess.Count} event/s from {buffer.ProducerName}");
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
                var message = (Message)incomingEvent;
                message.ProcessorBufferVersion = processorBufferVersion;
                message.StreamType = buffer.StreamType;

                buffer.EventsInProcessorByEcv.TryAdd(incomingEvent.EventCollectionVersion, new EventInProcessorBucket(incomingEvent));

                return incomingEvent;
                //this.bus.Publish(new NewIncomingEvents(incomingEvent));
            });

            this.bus.Publish(new NewIncomingEvents(streams));
            return true;
        }

        private void PollAndDispatch()
        {
            // This algorithm performs very well!
            var bufferPoolLength = this.bufferPool.Length;
            bool working;
            while (!this.stopping)
            {
                working = false;
                for (int i = 0; i < bufferPoolLength; i++)
                {
                    var buffer = this.bufferPool[i];
                    if (working)
                    {
                        this.TryFill(buffer);
                        this.TryFlush(buffer);
                    }
                    else
                    {
                        if (this.TryFill(buffer) | this.TryFlush(buffer))
                        {
                            working = true;
                        }
                    }
                }
                if (!working)
                    Thread.Sleep(1);
            }
        }

        public void Handle(PollResponseWasReceived message)
        {
            if (this.stopping)
                return;

            var response = message.Response;

            SubscriptionBuffer subscription;
            try
            {
                if (!this.onTheFlySubscriptionsDetected)
                    subscription = this.bufferPool.Where(s => s.StreamType == response.StreamType).Single();
                else
                {
                    subscription = this.bufferPool.Where(s => s.StreamType == response.StreamType).SingleOrDefault();
                    if (subscription == null)
                        subscription = this.onTheFlyBufferPool.Where(s => s.StreamType == response.StreamType).Single();
                }
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

            if (message.Response.ErrorDetected)
            {
                this.log.Error($"An error was detected while polling {message.Response.StreamType}. The error will be ignored and polling will continue with safety.");
                subscription.IsPolling = false;
                return;
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

                    // Not serialized, we just need to put in order...
                    : response.Events.OrderBy(e => e.EventCollectionVersion);

                subscription.CurrentBufferVersion = orderedEvents.Max(e => e.EventCollectionVersion);

                foreach (var e in orderedEvents)
                {
                    //this.log.Trace("Event: ECVersion {0}", e.EventCollectionVersion);
                    subscription.NewEventsQueue.Enqueue(e);
                }
            }

            subscription.ConsumerVersion = response.ConsumerVersion;
            subscription.ProducerVersion = response.ProducerVersion;

            subscription.IsPolling = false;

            if (this.log.Verbose)
            {
                var messageCount = response.IsSerialized ? response.NewRawEvents.Count() : response.Events.Count();
                if (messageCount > 0)
                    this.log.Trace($"{this.microserviceName} pulled {messageCount} event/s from {subscription.StreamType}");
            }
        }

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            var e = message.Event;
            if (!this.onTheFlySubscriptionsDetected)
                this.bufferPool
                .SingleOrDefault(s => s.StreamType == e.StreamType)
                ?.EventsInProcessorByEcv[e.EventCollectionVersion]
                .MarkEventAsProcessed();
            else
            {
                var sub = this.bufferPool.SingleOrDefault(s => s.StreamType == e.StreamType);
                if (sub == null)
                    sub = this.onTheFlyBufferPool.Single(s => s.StreamType == e.StreamType);
                sub.EventsInProcessorByEcv[e.EventCollectionVersion].MarkEventAsProcessed();
            }
        }

        public void Handle(IncomingEventIsPoisoned message)
        {
            lock (this.lockObjectForPoisonedSubs)
            {
                try
                {
                    SubscriptionBuffer poisonedSubscription;
                    if (!this.onTheFlySubscriptionsDetected)
                        poisonedSubscription = this.bufferPool.Where(s => s.StreamType == message.PoisonedEvent.StreamType).SingleOrDefault();
                    else
                    {
                        poisonedSubscription = this.bufferPool.Where(s => s.StreamType == message.PoisonedEvent.StreamType).SingleOrDefault();
                        if (poisonedSubscription == null)
                            poisonedSubscription = this.onTheFlyBufferPool.Where(s => s.StreamType == message.PoisonedEvent.StreamType).Single();
                    }

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
            this.stopSilently = message.StopSilently;
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
            this.bufferPool = this.repository.GetSubscriptions();

            lines.Add(string.Format("| Found {0} subscription/s", bufferPool.Count()));

            var subscriptionCount = 0;
            for (int i = 0; i < this.bufferPool.Length; i++)
            {
                var subscription = this.bufferPool[i];
                subscriptionCount += 1;
                lines.Add($"| --> Subscription {subscriptionCount} | Name: {subscription.StreamType} | {subscription.Url} | Buffer version: {subscription.CurrentBufferVersion}");
                this.RegisterOcassionallyConnectedSourceIfApplicable(subscription);
            }

            if (this.thread != null)
                throw new InvalidOperationException($"Already a thread running in poller of {this.microserviceName}.");

            this.thread = new Thread(this.PollAndDispatch) { IsBackground = true, Name = this.microserviceName + "_POLLER" };
            this.thread.Start();

            // Ensure to start everything;
            lines.Add($"| {this.microserviceName} poller started");
            this.log.Log($"Starting {this.microserviceName} poller...", lines.ToArray());

            this.bus.Publish(new EventPollerStarted());
        }

        private void RegisterOcassionallyConnectedSourceIfApplicable(SubscriptionBuffer subscription)
        {
            if (subscription.Token == Constants.InMemorySusbscriptionToken)
                this.mainPublisherRegistry.Register(new OcassionallyConnectedSource(subscription.StreamType, this.microserviceName));
        }

        public void Handle(AddNewSubscriptionOnTheFly message)
        {
            lock (this.lockObjectForOnTheFlySub)
            {
                if (this.repository.TryAddNewSubscriptionOnTheFly(message.StreamType, message.Url, message.Token))
                {

                    if (this.onTheFlyBufferPool == null)
                        this.onTheFlyBufferPool = new ConcurrentBag<SubscriptionBuffer>();
                    else
                        if (this.onTheFlyBufferPool.Any(x => x.StreamType == message.StreamType))
                        return;

                    var sub = new SubscriptionBuffer(message.StreamType, message.Url, message.Token, 0, false);
                    this.mainPublisherRegistry.Register(new OcassionallyConnectedSource(sub.StreamType, this.microserviceName));
                    this.onTheFlyBufferPool.Add(sub);

                    if (this.onTheFlySubscriptionsDetected)
                        return;

                    this.onTheFlySubscriptionsDetected = true;
                    if (this.onTheFlyThread != null)
                        throw new InvalidOperationException($"Already a thread of on the fly subscription running in poller of {this.microserviceName}.");

                    this.onTheFlyThread = new Thread(() =>
                    {
                        // This algorithm performs very well!
                        bool working;
                        while (!this.stopping)
                        {
                            working = false;
                            foreach (var buffer in this.onTheFlyBufferPool)
                            {
                                if (working)
                                {
                                    this.TryFill(buffer);
                                    this.TryFlush(buffer);
                                }
                                else
                                {
                                    if (this.TryFlush(buffer) | this.TryFill(buffer))
                                    {
                                        working = true;
                                    }
                                }
                            }
                            if (!working)
                                Thread.Sleep(1);
                        }
                    })
                    {
                        IsBackground = true,
                        Name = this.microserviceName + "_POLLER_ON-THE-FLY-SUBSCRIPTION"
                    };
                    this.onTheFlyThread.Start();
                }

                else
                    return;
            }
        }

        protected override void OnStopping()
        {
            // Ensure to stop everything;
            if (!this.stopSilently)
                this.log.Log($"{this.microserviceName} poller stopped");
            this.bus.Publish(new EventPollerStopped());
        }

        public IMonitoredSubscription[] GetSubscriptionsMetrics() => this.bufferPool;

        public List<SubscriptionBuffer> GetBufferPool()
        {
            var pool = new List<SubscriptionBuffer>();
            pool.AddRange(this.bufferPool);
            return pool;
        }

        protected override void RegisterHandlersInBus(IBusRegistry bus)
        {
            bus.Register<StartEventPoller>(this);
            bus.Register<StopEventPoller>(this);
            bus.Register<PollResponseWasReceived>(this);
            bus.Register<IncomingEventHasBeenProcessed>(this);
            bus.Register<IncomingEventIsPoisoned>(this);
            bus.Register<AddNewSubscriptionOnTheFly>(this);
        }
    }
}
