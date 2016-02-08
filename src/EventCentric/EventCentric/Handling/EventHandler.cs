using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Handling
{
    /// <summary>
    /// An event processor
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEventSourced"/> aggregate.</typeparam>
    /// <remarks>
    /// If the aggregate needs to process a command and needs a Domain Service, it should be injected with 
    /// a custum made container, with a generic injection for dependency resolution. It should be best 
    /// initialiezed lazyly, as the logger of Greg Young's Event Store.
    /// </remarks>
    public abstract class EventHandlerOf<T> : NodeWorker,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<NewIncomingEvents>,
        IMessageHandler<NewIncomingEvent>,
        IHandles<IEvent>
            where T : class, IEventSourced
    {
        private readonly IEventStore<T> store;
        private readonly Func<Guid, T> newAggregateFactory;
        private readonly ConcurrentDictionary<string, object> streamLocksById;
        protected readonly ConcurrentBag<Guid> poisonedStreams;

        protected EventHandlerOf(IBus bus, ILogger log, IEventStore<T> store)
            : base(bus, log)
        {
            Ensure.NotNull(store, "store");

            this.store = store;

            this.streamLocksById = new ConcurrentDictionary<string, object>();
            this.poisonedStreams = new ConcurrentBag<Guid>();

            // New aggregate
            var constructor = typeof(T).GetConstructor(new[] { typeof(Guid) });
            Ensure.CastIsValid(constructor, "Type T must have a constructor with the following signature: .ctor(Guid)");

            this.newAggregateFactory = (id) => (T)constructor.Invoke(new object[] { id });
        }

        public void Handle(NewIncomingEvent message)
        {
#if DEBUG
            this.log.Trace($"Processing an event fom {message.IncomingEvent.StreamType}");
#endif

            this.HandleGracefully(message.IncomingEvent);
        }

        public void Handle(NewIncomingEvents message)
        {
#if DEBUG
            var traces = new System.Collections.Generic.List<string>();
            traces.Add($"Processing {message.IncomingEvents.Count()} event/s as follows:");
            foreach (var incomingEvent in message.IncomingEvents)
            {
                traces.Add($"   Processor buffer version {incomingEvent.ProcessorBufferVersion} | Event collection version {incomingEvent.EventCollectionVersion} | Stream {incomingEvent.StreamId} version {incomingEvent.Version}");
            }
            this.log.Trace(traces.ToArray());
#endif

            foreach (var incomingEvent in message.IncomingEvents)
            {
                this.HandleGracefully(incomingEvent);
            }
        }

        private void HandleGracefully(IEvent incomingEvent)
        {
            try
            {
#if DEBUG
                this.log.Trace($"Processor is now handling message '{incomingEvent.GetType().Name}' with id {incomingEvent.EventId}");
#endif
                ((dynamic)this).Handle((dynamic)incomingEvent);
#if DEBUG
                this.log.Trace($"Processor successfully handled message '{incomingEvent.GetType().Name}' with id {incomingEvent.EventId}");
#endif
            }
            catch (Exception ex)
            {
                var exception = new PoisonMessageException("An error ocurred in Event Processor while processing a message. The message will be marked as poisoned in order to review it. Maybe is just a dynamic binding error.", ex);

                this.log.Error(exception, $"Poison message of type {incomingEvent.GetType().Name} detected in Event Processor");

                this.bus.Publish(
                    new IncomingEventIsPoisoned(incomingEvent, exception));
            }
        }

        public void Handle(StartEventProcessor message)
        {
            base.Start();
        }

        public void Handle(StopEventProcessor message)
        {
            base.Stop();
        }

        protected override void OnStarting()
        {
            base.log.Trace("Event processor started");
            base.bus.Publish(new EventProcessorStarted());

        }

        protected override void OnStopping()
        {
            this.log.Trace("Event processor stopped");
            this.bus.Publish(new EventProcessorStopped());
        }

        /// <summary>
        /// Mark as ignored in the inbox table and in the subscription table. 
        /// This node is not subscribed to this event, but is interested in other events that
        /// happened in the source. Or is DUPLICATE
        /// </summary>
        /// <param name="@event">The <see cref="IEvent"/> to be igonred.</param>
        protected void Ignore(IEvent incomingEvent)
        {
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        /// <summary>
        /// Safely handles the message with locking.
        /// </summary>
        /// <param name="streamId">The id of the stream, for locking purposes.</param>
        /// <param name="handle">The handling action.</param>
        private void HandleSafelyWithStreamLocking(Guid streamId, IEvent incomingEvent, Action handle)
        {
            /***************************************************** 

               This was the old way to get a lock.
              
               this.streamLocksById.TryAdd(id, new object());
               lock (this.streamLocksById.TryGetValue(id))

            ******************************************************/

            lock (this.streamLocksById.GetOrAdd(streamId.ToString(), new object()))
            {
                try
                {
                    // Handle only if is not poisoned
                    if (!this.poisonedStreams.Where(p => p == streamId).Any())
                        handle();
                }
                catch (StreamNotFoundException ex)
                {
                    // we igonore it, just to protect our servers to get down.
                    this.log.Error(ex, $"The stream {streamId} was not found. Ignoring message. You can retry by reseting the subscription table.");
                    this.Ignore(incomingEvent);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (this.store.IsDuplicate(incomingEvent.EventId))
                        {
                            this.Ignore(incomingEvent);
                            return;
                        }
                    }
                    catch (Exception otherEx)
                    {
                        this.bus.Publish(
                        new FatalErrorOcurred(new FatalErrorException($"Error while trying to check if an event is duplicate", otherEx)));
                    }

                    var exception = new PoisonMessageException("Poison message detected in Event Processor", ex);

                    this.log.Error(exception, $"Poison message of type {incomingEvent.GetType().Name} detected in Event Processor");

                    this.bus.Publish(
                        new IncomingEventIsPoisoned(incomingEvent, exception));

                    // If an error ocurred, flag it in order to abort any subsecuent attemps to update the stream that
                    // could not process a previous event.
                    this.poisonedStreams.Add(streamId);
                }
            }
        }

        private void PublishIncomingEventHasBeenProcessed(IEvent incomingEvent)
        {
            this.bus.Publish(new IncomingEventHasBeenProcessed(incomingEvent.StreamType, incomingEvent.EventCollectionVersion));
        }

        protected void HandleInNewStreamIfNotExists(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, incomingEvent, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        /// <summary>
        /// Creates a new stream in the store.
        /// </summary>
        /// <param name="id">The id of the new stream. A brand new computed <see cref="Guid"/>.</param>
        /// <param name="@event">The first message that the new aggregate will process.</param>
        protected void HandleInNewStream(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, incomingEvent, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        /// <summary>
        /// Gets a stream from the store to hydrate the event sourced aggregate of <see cref="T"/>.
        /// </summary>
        /// <param name="streamId">The id of the stream.</param>
        /// <param name="@event">The event to be handled by the aggregate of <see cref="T"/>.</param>
        protected void HandleInExistingStream(Guid streamId, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(streamId, incomingEvent, () =>
            {
                var aggregate = this.store.Get(streamId);
                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent);
            var version = this.store.Save(aggregate, incomingEvent);
            this.bus.Publish(new EventStoreHasBeenUpdated(version));
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        public void Handle(IEvent message)
        {
#if DEBUG
            this.log.Trace($"Ignoring event of type {message.GetType().FullName}");
#endif
            this.Ignore(message);
        }
    }
}
