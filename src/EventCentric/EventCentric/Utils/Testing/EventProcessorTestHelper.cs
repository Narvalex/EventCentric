using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
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
        private readonly EventStoreStub eventStore;

        public EventProcessorTestHelper(Guid streamId)
        {
            this.eventStore = new EventStoreStub(streamId);
            this.Bus = new BusStub();
            this.Log = new ConsoleLogger();
        }

        public BusStub Bus { get; }

        public ILogger Log { get; }

        public IEventStore<TAggregate> EventStore => this.eventStore;

        public TProcessor Processor { get; private set; }

        public void Setup(TProcessor processor) => this.Processor = processor;

        public void Given(params IEvent[] eventStream)
            => this.eventStore
                   .ActualStream
                   .AddRange(eventStream);

        public void Given(IMemento memento)
            => this.eventStore.ActualSnapshot = memento;

        public TAggregate When(IEvent @event)
        {
            this.Processor.Handle(new NewIncomingEvent(@event));
            return this.eventStore.UpdatedAggregate;
        }

        public TMemento ThenPersistsNewSerializedMemento<TMemento>() => (TMemento)this.eventStore.UpdatedSnapshot;

        public IEnumerable<IEvent> ThenPersistsNewSerializedEvents() => this.eventStore.AppendedStream;

        public class BusStub : IBus, IBusRegistry
        {
            public readonly List<IMessage> Messages = new List<IMessage>();

            public void Publish(IMessage message)
                => this.Messages.Add(message);

            public void Register(IWorker worker)
            { }
        }

        public class EventStoreStub : IEventStore<TAggregate>
        {
            private readonly Guid streamId;
            public ITextSerializer serializer = new JsonTextSerializer();

            public readonly List<IEvent> ActualStream = new List<IEvent>();
            public IMemento ActualSnapshot = null;

            public readonly List<IEvent> AppendedStream = new List<IEvent>();
            public IMemento UpdatedSnapshot = null;

            public TAggregate UpdatedAggregate = null;

            private readonly Func<Guid, IEnumerable<IEvent>, TAggregate> aggregateFactory;
            private readonly Func<Guid, IMemento, TAggregate> originatorAggregateFactory;

            public EventStoreStub(Guid streamId)
            {
                this.streamId = streamId;

                var fromMementoConstructor = typeof(TAggregate).GetConstructor(new[] { typeof(Guid), typeof(IMemento) });
                Ensure.CastIsValid(fromMementoConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IMemento)");
                this.originatorAggregateFactory = (id, memento) => (TAggregate)fromMementoConstructor.Invoke(new object[] { id, memento });

                var fromStreamConstructor = typeof(TAggregate).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IEvent>) });
                Ensure.CastIsValid(fromStreamConstructor, "Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IEvent>)");
                this.aggregateFactory = (id, streamOfEvents) => (TAggregate)fromStreamConstructor.Invoke(new object[] { id, streamOfEvents });
            }

            TAggregate IEventStore<TAggregate>.Find(Guid id)
            {
                if (id != this.streamId)
                    throw new StreamNotFoundException(id, "Test");

                if (this.ActualSnapshot != null)
                    return this.originatorAggregateFactory(id, this.ActualSnapshot);

                return this.ActualStream.Count > 0 ? this.aggregateFactory.Invoke(id, this.ActualStream)
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
                var events = eventSourced.PendingEvents
                            .ToList()
                            .Select(e =>
                            {
                                var serialized = this.serializer.Serialize(e);
                                return this.serializer.Deserialize<IEvent>(serialized);
                            });

                this.AppendedStream.AddRange(events);
                this.UpdatedAggregate = eventSourced;
                var memento = ((IMementoOriginator)eventSourced).SaveToMemento();
                var serializedMemento = this.serializer.Serialize(memento);
                this.UpdatedSnapshot = this.serializer.Deserialize<IMemento>(serializedMemento);

                return events.Max(e => e.Version);
            }
        }
    }
}
