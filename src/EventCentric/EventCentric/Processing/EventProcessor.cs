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
        IMessageHandler<NewIncomingEvents>,
        IMessageHandler<NewIncomingEvent>
            where T : class, IEventSourced
    {
        private readonly IEventStore<T> store;
        private readonly Func<Guid, T> newAggregateFactory;
        private readonly ConcurrentDictionary<string, object> streamLocksById;
        protected readonly ConcurrentBag<Guid> poisonedStreams;

        public EventProcessor(IBus bus, ILogger log, IEventStore<T> store)
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
                ((dynamic)this).Receive((dynamic)incomingEvent);
#if DEBUG
                this.log.Trace($"Processor successfully handled message '{incomingEvent.GetType().Name}' with id {incomingEvent.EventId}");
#endif
            }
            catch (Exception ex)
            {
                var exception = new PoisonMessageException("Poison message detected in Event Processor", ex);

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
        /// happened in the source.
        /// </summary>
        /// <param name="@event">The <see cref="IEvent"/> to be igonred.</param>
        protected void Ignore(IEvent incomingEvent)
        {
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        /// <summary>
        /// Safely handles the message with locking.
        /// </summary>
        /// <param name="id">The id of the stream, for locking purposes.</param>
        /// <param name="handle">The handling action.</param>
        private void HandleSafelyWithStreamLocking(Guid id, Action handle)
        {
            /***************************************************** 

               This was the old way to get a lock.
              
               this.streamLocksById.TryAdd(id, new object());
               lock (this.streamLocksById.TryGetValue(id))

            ******************************************************/

            lock (this.streamLocksById.GetOrAdd(id.ToString(), new object()))
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

        #region CreateNewStreamIfNotExistsAndProcess

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15);
            });
        }

        protected void CreateNewStreamIfNotExistsAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15, IDomainService s16)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Find(id);
                if (aggregate == null)
                    aggregate = this.newAggregateFactory(id);

                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16);
            });
        }

        #endregion

        #region CreateNewStreamAndProcess

        /// <summary>
        /// Creates a new stream in the store.
        /// </summary>
        /// <param name="id">The id of the new stream. A brand new computed <see cref="Guid"/>.</param>
        /// <param name="@event">The first message that the new aggregate will process.</param>
        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15);
            });
        }

        protected void CreateNewStreamAndProcess(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15, IDomainService s16)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.newAggregateFactory(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16);
            });
        }

        #endregion

        #region Process
        /// <summary>
        /// Gets a stream from the store to hydrate the event sourced aggregate of <see cref="T"/>.
        /// </summary>
        /// <param name="id">The id of the stream.</param>
        /// <param name="@event">The event to be handled by the aggregate of <see cref="T"/>.</param>
        protected void Process(Guid id, IEvent incomingEvent)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15);
            });
        }

        protected void Process(Guid id, IEvent incomingEvent, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15, IDomainService s16)
        {
            this.HandleSafelyWithStreamLocking(id, () =>
            {
                var aggregate = this.store.Get(id);
                this.HandleAndUpdate(incomingEvent, aggregate, s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16);
            });
        }

        #endregion

        /// <summary>
        /// Handles and event and updates the stream
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="@event">The event.</param>
        /// <returns>The updated stream version.</returns>
        private void UpdateStream(T aggregate, IEvent incomingEvent)
        {
            var version = this.store.Save(aggregate, incomingEvent);
            this.bus.Publish(new EventStoreHasBeenUpdated(version));
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }

        #region HandleAndUpdate
        private void HandleAndUpdate(IEvent incomingEvent, T aggregate)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14, (dynamic)s15);
            this.UpdateStream(aggregate, incomingEvent);
        }

        private void HandleAndUpdate(IEvent incomingEvent, T aggregate, IDomainService s1, IDomainService s2, IDomainService s3, IDomainService s4, IDomainService s5, IDomainService s6, IDomainService s7, IDomainService s8, IDomainService s9, IDomainService s10, IDomainService s11, IDomainService s12, IDomainService s13, IDomainService s14, IDomainService s15, IDomainService s16)
        {
            ((dynamic)aggregate).Handle((dynamic)incomingEvent, (dynamic)s1, (dynamic)s2, (dynamic)s3, (dynamic)s4, (dynamic)s5, (dynamic)s6, (dynamic)s7, (dynamic)s8, (dynamic)s9, (dynamic)s10, (dynamic)s11, (dynamic)s12, (dynamic)s13, (dynamic)s14, (dynamic)s15, (dynamic)s16);
            this.UpdateStream(aggregate, incomingEvent);
        }
        #endregion
    }
}
