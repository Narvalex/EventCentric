using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Utils;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Handling
{
    public abstract class HandlerOf<TEventSourced> : MicroserviceWorker,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<NewIncomingEvents>,
        IHandles<IEvent>
            where TEventSourced : class, IEventSourced
    {
        private readonly IEventStore<TEventSourced> store;
        private readonly Func<Guid, TEventSourced> newAggregateFactory;
        private readonly ConcurrentDictionary<string, object> streamLocksById;
        protected readonly ConcurrentBag<Guid> poisonedStreams;

        public HandlerOf(IBus bus, ILogger log, IEventStore<TEventSourced> store)
            : base(bus, log)
        {
            Ensure.NotNull(store, "store");

            this.store = store;

            this.streamLocksById = new ConcurrentDictionary<string, object>();
            this.poisonedStreams = new ConcurrentBag<Guid>();

            // New aggregate
            var constructor = typeof(TEventSourced).GetConstructor(new[] { typeof(Guid) });
            Ensure.CastIsValid(constructor, "Type T must have a constructor with the following signature: .ctor(Guid)");

            this.newAggregateFactory = (id) => (TEventSourced)constructor.Invoke(new object[] { id });
        }

        public void AdHocHandle(IEvent @event)
        {
#if DEBUG
            this.log.Trace($"Processing an event fom {@event.StreamType}");
#endif

            this.HandleGracefully(@event);
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
                dynamic me = this;
                IMessageHandling handling = me.Handle((dynamic)incomingEvent);

                if (handling.ShouldBeIgnored)
                {
                    this.Ignore(incomingEvent);
                    return;
                }

                /***************************************************** 
                    This was the old way to get a lock.
              
                    this.streamLocksById.TryAdd(id, new object());
                    lock (this.streamLocksById.TryGetValue(id))
    
                ******************************************************/
                lock (this.streamLocksById.GetOrAdd(handling.StreamId.ToString(), new object()))
                {
                    try
                    {
                        if (!this.poisonedStreams.Where(p => p == handling.StreamId).Any())
                        {
                            this.HandleAndSaveChanges(incomingEvent, handling);
                        }
                    }
                    catch (StreamNotFoundException ex)
                    {
                        // we igonore it, just to protect our servers to get down.
                        this.log.Error(ex, $"The stream {handling.StreamId} was not found. Ignoring message. You can retry by reseting the subscription table.");
                        this.Ignore(incomingEvent);
                    }
                    catch (RuntimeBinderException ex)
                    {
                        this.log.Error(ex, $"The state does not have an overload to update when event {incomingEvent.GetType().Name} happened. Did you forget to write a When(IEvent event) method?");
                        this.poisonedStreams.Add(handling.StreamId);
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        this.log.Error(ex, $"An error was detected while processing a message. Now we will try to check if it is duplicate or the snapshot is currupted");

                        // Maybe is duplicate
                        try
                        {
                            if (this.store.IsDuplicate(incomingEvent.EventId))
                            {
                                this.Ignore(incomingEvent);
                                return;
                            }
                        }
                        catch (Exception duplicateEx)
                        {
                            this.log.Error(duplicateEx, "An error ocurred while checking if incoming message is duplicate.");
                        }

                        // Or maybe the snapshot is corrupted
                        try
                        {
                            this.store.DeleteSnapshot(handling.StreamId);
                            this.HandleAndSaveChanges(incomingEvent, handling);
                            return;
                        }
                        catch (Exception deleteSnapshotEx)
                        {
                            this.log.Error(deleteSnapshotEx, "An error ocurred while deleting snapshot and trying to re-process a message.");
                        }

                        this.poisonedStreams.Add(handling.StreamId);
                        throw ex;
                    }
                }
#if DEBUG
                this.log.Trace($"Processor successfully handled message '{incomingEvent.GetType().Name}' with id {incomingEvent.EventId}");
#endif
            }
            catch (Exception ex)
            {
                var exception = new PoisonMessageException("An error ocurred in Event Processor while processing a message. The message will be marked as poisoned in order to review it. Maybe is just a dynamic binding error.", ex);

                this.bus.Publish(new IncomingEventIsPoisoned(incomingEvent, exception));

                this.log.Error(exception, $"Poison message of type {incomingEvent.GetType().Name} detected in Event Processor");
            }
        }

        private void HandleAndSaveChanges(IEvent incomingEvent, IMessageHandling handling)
        {
            var eventSourced = handling.Handle.Invoke();
            this.store.Save((TEventSourced)eventSourced, incomingEvent);
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

        private IMessageHandling BuildHandlingInvocation(Guid streamId, Func<TEventSourced, TEventSourced> handle, Func<TEventSourced> aggregateFactory)
            => new MessageHandling(false, streamId, () => handle.Invoke(aggregateFactory.Invoke()));

        protected IMessageHandling FromNewStreamIfNotExists(Guid id, Func<TEventSourced, TEventSourced> handle)
            => this.BuildHandlingInvocation(id, handle, () =>
                 {
                     var aggregate = this.store.Find(id);
                     return aggregate != null ? aggregate : this.newAggregateFactory(id);
                 });

        protected IMessageHandling FromNewStream(Guid id, Func<TEventSourced, TEventSourced> handle)
            => this.BuildHandlingInvocation(id, handle, () => this.newAggregateFactory(id));

        protected IMessageHandling FromStream(Guid streamId, Func<TEventSourced, TEventSourced> handle)
           => this.BuildHandlingInvocation(streamId, handle, () => this.store.Get(streamId));

        public IMessageHandling Handle(IEvent message)
        {
#if DEBUG
            this.log.Trace($"Ignoring event of type {message.GetType().FullName}");
#endif
            return new MessageHandling(true, default(Guid), () => null);
        }

        /// <summary>
        /// Mark as ignored in the inbox table and in the subscription table. 
        /// This node is not subscribed to this event, but is interested in other events that
        /// happened in the source. Or is DUPLICATE
        /// </summary>
        /// <param name="@event">The <see cref="IEvent"/> to be igonred.</param>
        private void Ignore(IEvent incomingEvent)
        {
            this.bus.Publish(new IncomingEventsHasBeenProcessed(
                new List<Tuple<string, long>>() { new Tuple<string, long>(incomingEvent.StreamType, incomingEvent.EventCollectionVersion) }));
        }
    }
}
