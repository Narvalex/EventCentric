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
using System.Threading;

namespace EventCentric.Handling
{
    public abstract class HandlerOf<TEventSourced> : MicroserviceWorker,
        IMessageHandler<StartEventProcessor>,
        IMessageHandler<StopEventProcessor>,
        IMessageHandler<NewIncomingEvents>,
        IHandles<IEvent>
            where TEventSourced : class, IEventSourced
    {
        private static readonly string name = typeof(TEventSourced).Name;
        private readonly IEventStore<TEventSourced> store;
        private readonly Func<Guid, TEventSourced> newAggregateFactory;
        private readonly ConcurrentDictionary<string, object> streamLocksById;
        private readonly ConcurrentBag<Guid> poisonedStreams;

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
            if (this.log.Verbose)
                this.log.Trace($"{name} is handling a message of type {@event.GetType().Name} from {@event.StreamType}");

            this.HandleGracefully(@event);
        }

        public void Handle(NewIncomingEvents message)
        {
            if (this.log.Verbose)
                this.log.Trace($"{name} is now handling {message.IncomingEvents.Count()} message/s");

            var streams = message.IncomingEvents.GroupBy(e => e.StreamId);

            foreach (var stream in streams)
            {
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(_ =>
                {
                    foreach (var e in stream)
                        this.HandleGracefully(e);
                }),
                null);
            }
        }

        private void HandleGracefully(IEvent incomingEvent)
        {
            try
            {
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

                        if (this.log.Verbose)
                            this.log.Trace($"{name} successfully handled message of type {incomingEvent.GetType().Name}");
                    }
                    catch (StreamNotFoundException ex)
                    {
                        // we igonore it, just to protect our servers to get down.
                        this.log.Error(ex, $"The stream {handling.StreamId} was not found. Ignoring message. You can retry by reseting the subscription table.");
                        this.Ignore(incomingEvent);
                    }
                    catch (RuntimeBinderException ex)
                    {
                        var list = new List<string>();
                        list.Add($"The state of {name} seems like it does not have an orveload to update when message of type {incomingEvent.GetType().Name} is processed. Did you forget to write a When-Event-like method?");
                        list.Add("-----------------------------------------------------------------------------------");
                        this.log.Error(ex, "", list.ToArray());
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
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
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
            base.log.Log($"{typeof(TEventSourced).Name} handler started");
            base.bus.Publish(new EventHandlerStarted());

        }

        protected override void OnStopping()
        {
            this.log.Log($"{name} handler stopped");
            this.bus.Publish(new EventProcessorStopped());
        }

        private IMessageHandling BuildHandlingInvocation(Guid streamId, Func<TEventSourced, TEventSourced> handle, Func<TEventSourced> aggregateFactory)
            => new MessageHandling(false, streamId, () => handle.Invoke(aggregateFactory.Invoke()));

        private void PublishIncomingEventHasBeenProcessed(IEvent incomingEvent)
        {
            this.bus.Publish(new IncomingEventHasBeenProcessed(incomingEvent.StreamType, incomingEvent.EventCollectionVersion));
        }

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
            if (this.log.Verbose)
                this.log.Trace($"{name} is automatically ignoring message of type {message.GetType().Name} because no handling method where found");
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
            if (this.log.Verbose)
                this.log.Trace($"{name} is deliberately ignoring message of type {incomingEvent.GetType().Name}");
            this.PublishIncomingEventHasBeenProcessed(incomingEvent);
        }
    }
}
