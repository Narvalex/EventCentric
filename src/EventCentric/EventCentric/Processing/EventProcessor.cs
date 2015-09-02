using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace EventCentric.Processing
{
    /// <summary>
    /// An Event Processor
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEventSourced"/> aggregate.</typeparam>
    /// <remarks>
    /// If the aggregate needs to process a command and needs a Domain Service, it should be injected with 
    /// a custum made container, with a generic injection for dependency resolution. It should be best 
    /// initialiezed lazyly, as the logger of Greg Young's Event Store.
    /// </remarks>
    public abstract class EventProcessor<T> : FSM,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<NewIncomingEvent>
            where T : class, IEventSourced
    {
        private readonly IEventStore<T> store;
        private readonly Func<Guid, T> newAggregateFactory;
        protected readonly ConcurrentDictionary<Guid, object> streamLocksById;
        protected readonly ConcurrentBag<Guid> poisonedStreams;

        public EventProcessor(IBus bus, ILogger log, IEventStore<T> store)
            : base(bus, log)
        {
            Ensure.NotNull(store, "store");

            this.store = store;

            this.streamLocksById = new ConcurrentDictionary<Guid, object>();
            this.poisonedStreams = new ConcurrentBag<Guid>();

            // New aggregate
            var constructor = typeof(T).GetConstructor(new[] { typeof(Guid) });
            Ensure.CastIsValid(constructor, "Type T must have a constructor with the following signature: .ctor(Guid)");

            this.newAggregateFactory = (id) => (T)constructor.Invoke(new object[] { id });
        }

        public void Handle(NewIncomingEvent message)
        {
            try
            {
                var incomingEvent = message.IncomingEvent;
                if (this.store.IncomingEventIsDuplicate(incomingEvent.EventId))
                {
                    this.Ignore(incomingEvent);
                    return;
                }

                ((dynamic)this).Handle((dynamic)incomingEvent);
            }
            catch (Exception ex)
            {
                this.bus.Publish(
                    new IncomingEventIsPoisoned(
                        message.IncomingEvent,
                        new PoisonMessageException("Poison message detected in Event Processor", ex)));
            }
        }

        public void Handle(StartEventProcessor message)
        {
            base.Start();
            base.bus.Publish(new EventProcessorStarted());
        }

        public void Handle(StopEventProcessor message)
        {
            base.Stop();
            this.bus.Publish(new EventProcessorStopped());
        }

        /// <summary>
        /// Creates a new stream in the store.
        /// </summary>
        /// <param name="id">The id of the new stream. A brand new computed <see cref="Guid"/>.</param>
        /// <param name="@event">The first message that the new aggregate will process.</param>
        protected void CreateNewStream(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleEventAndAppendToStore(aggregate, incomingEvent);
            });
        }

        protected void CreateNewStreamIfNotExists(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleEventAndAppendToStore(aggregate, incomingEvent);
            });
        }

        /// <summary>
        /// Gets a stream from the store to hydrate the event sourced aggregate of <see cref="T"/>.
        /// </summary>
        /// <param name="id">The id of the stream.</param>
        /// <param name="@event">The event to be handled by the aggregate of <see cref="T"/>.</param>
        protected void Handle(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleEventAndAppendToStore(aggregate, incomingEvent);
            });
        }

        /// <summary>
        /// Mark as ignored in the inbox table and in the subscription table. 
        /// This node is not subscribed to this event, but is interested in other events that
        /// happened in the source.
        /// </summary>
        /// <param name="@event">The <see cref="IEvent"/> to be igonred.</param>
        protected void Ignore(IEvent incomingEvent)
        {
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        /// <summary>
        /// Handles and event and updates the stream
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="@event">The event.</param>
        /// <returns>The updated stream version.</returns>
        private void HandleEventAndAppendToStore(T aggregate, IEvent incomingEvent)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent);
            var version = this.store.Save(aggregate, incomingEvent);
            this.bus.Publish(new EventStoreHasBeenUpdated(version));
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        /// <summary>
        /// Safely handles the message with locking.
        /// </summary>
        /// <param name="id">The id of the stream, for locking purposes.</param>
        /// <param name="handle">The handling action.</param>
        private void HandleSafelyWithStreamLocking(Guid id, Action handle)
        {
            this.streamLocksById.TryAdd(id, new object());
            lock (this.streamLocksById.TryGetValue(id))
            {
                try
                {
                    // Handle only if is not poisoned
                    if (!this.poisonedStreams.Where(p => p == id).Any())
                        handle();
                }
                catch (Exception ex)
                {
                    // If an error ocurred, flag it in order to abort any subsecuent attemps to update the stream that
                    // could not process a previous event.
                    this.poisonedStreams.Add(id);
                    throw ex;
                }
            }
        }

        private void PublishIncomingEventHasBeenProcessed(IEvent incomingEvent)
        {
            this.bus.Publish(new IncomingEventHasBeenProcessed(incomingEvent.StreamType, incomingEvent.EventCollectionVersion));
        }
    }
}
