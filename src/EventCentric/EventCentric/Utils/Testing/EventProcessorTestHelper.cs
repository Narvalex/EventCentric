using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging.Events;
using EventCentric.Processing;
using EventCentric.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Utils.Testing
{
    public class EventProcessorTestHelper<TAggregate, TProcessor>
        where TAggregate : class, IEventSourced
        where TProcessor : EventProcessor<TAggregate>
    {
        private readonly ITextSerializer serializer;
        private readonly EventStoreStub store;

        public EventProcessorTestHelper()
        {
            this.serializer = new JsonTextSerializer();
            this.store = new EventStoreStub(this.serializer);
            this.Bus = new BusStub();
            this.Log = new ConsoleLogger();

            // to check default values of an aggregate
            var constructor = typeof(TAggregate).GetConstructor(new[] { typeof(Guid) });
            Ensure.CastIsValid(constructor, "Type T must have a constructor with the following signature: .ctor(Guid)");

            this.store.Aggregate = (TAggregate)constructor.Invoke(new object[] { Guid.Empty });
        }

        public BusStub Bus { get; }

        public ILogger Log { get; }

        public IEventStore<TAggregate> Store => this.store;

        public TProcessor Processor { get; private set; }

        public TAggregate Aggregate => this.store.Aggregate;

        public void Setup(TProcessor processor) => this.Processor = processor;

        public void Given(Guid streamId, params IEvent[] eventStream)
        {
            this.store
                   .Stream
                   .AddRange(eventStream.Select(e => this.serializer.SerializeAndDeserialize(e)));
            this.store.streamId = streamId;
        }

        public void Given(Guid streamId, IMemento memento)
        {
            this.store.Snapshot = this.serializer.SerializeAndDeserialize(memento);
            this.store.streamId = streamId;
        }

        public void Given(Guid streamId, Func<IMemento, bool> mementoEqualityChecker, params IEvent[] eventStream)
        {
            this.Given(streamId, eventStream);

            foreach (var e in eventStream)
                ((dynamic)this.Aggregate).On((dynamic)e);

            var mementoFromEventStreamRehydration = this.serializer.SerializeAndDeserialize(((IMementoOriginator)this.Aggregate).SaveToMemento());

            // we check first if the memento is what expected.
            if (mementoEqualityChecker.Invoke(mementoFromEventStreamRehydration))
            {
                // we check secondly if the aggregate can rehydrate from given memento
                this.Given(streamId, mementoFromEventStreamRehydration);
                return;
            }

            throw new InvalidOperationException("The memento from event stream rehydration is not equal to expected");
        }

        public TAggregate When(IEvent @event)
        {
            this.Processor.Handle(new NewIncomingEvent(this.serializer.SerializeAndDeserialize(@event)));
            return this.store.Aggregate;
        }

        public TMemento ThenPersistsNewSerializedMemento<TMemento>() => (TMemento)this.store.Snapshot;

        public IEnumerable<IEvent> ThenPersistsNewSerializedEvents() => this.store.Stream;

        public class EventStoreStub : IEventStore<TAggregate>
        {
            internal Guid streamId;
            internal ITextSerializer serializer;

            internal readonly List<IEvent> Stream = new List<IEvent>();
            internal IMemento Snapshot = null;

            internal TAggregate Aggregate = null;

            private readonly Func<Guid, IEnumerable<IEvent>, TAggregate> aggregateFactory;
            private readonly Func<Guid, IMemento, TAggregate> originatorAggregateFactory;

            public EventStoreStub(ITextSerializer serializer)
            {
                this.serializer = serializer;

                var fromMementoConstructor = typeof(TAggregate).GetConstructor(new[] { typeof(Guid), typeof(IMemento) });
                Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
                this.originatorAggregateFactory = (id, memento) => (TAggregate)fromMementoConstructor.Invoke(new object[] { id, memento });

                var fromStreamConstructor = typeof(TAggregate).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
                Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
                this.aggregateFactory = (id, streamOfEvents) => (TAggregate)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });
            }

            TAggregate IEventStore<TAggregate>.Find(Guid id)
            {
                if (this.Snapshot != null)
                    return this.originatorAggregateFactory(id, this.Snapshot);

                return this.Stream.Count > 0 ? this.aggregateFactory.Invoke(id, this.Stream)
                                     : default(TAggregate);
            }

            TAggregate IEventStore<TAggregate>.Get(Guid id)
            {
                var aggregate = ((IEventStore<TAggregate>)this).Find(id);
                if (Equals(aggregate, default(TAggregate)))
                    throw new StreamNotFoundException(id, "Test");

                return aggregate;
            }

            long IEventStore<TAggregate>.Save(TAggregate eventSourced, IEvent incomingEvent)
            {
                this.streamId = eventSourced.Id;

                var events = eventSourced
                            .PendingEvents
                            .ToList()
                            .Select(e => this.serializer.SerializeAndDeserialize(e));

                this.Stream.AddRange(events);
                this.Aggregate = eventSourced;
                var memento = ((IMementoOriginator)eventSourced).SaveToMemento();
                this.Snapshot = this.serializer.SerializeAndDeserialize(memento);

                return events.Max(e => e.Version);
            }
        }
    }
}
