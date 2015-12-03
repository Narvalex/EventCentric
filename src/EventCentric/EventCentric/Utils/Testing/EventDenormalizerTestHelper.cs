using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging.Events;
using EventCentric.Processing;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using System;

namespace EventCentric.Utils.Testing
{
    public class EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext>
        where TAggregate : class, IEventSourced
        where TProcessor : EventProcessor<TAggregate>
        where TDbContext : IEventStoreDbContext
    {
        private object dao;
        private readonly ITextSerializer serializer;

        public EventDenormalizerTestHelper(string connectionString)
        {
            this.serializer = new JsonTextSerializer();
            this.Bus = new BusStub();
            this.Log = new ConsoleLogger();
            this.Time = new LocalTimeProvider();
            this.Guid = new SequentialGuid();
            this.NodeName = NodeNameResolver.ResolveNameOf<TAggregate>();

            var dbContextConstructor = typeof(TDbContext).GetConstructor(new[] { typeof(string) });
            Ensure.CastIsValid(dbContextConstructor, "Type TDbContext must have a constructor with the following signature: ctor(string)");
            this.EventStoreDbContextFactory = isReadOnly => (TDbContext)dbContextConstructor.Invoke(new object[] { connectionString });
            this.ReadModelDbContextFactory = () => (TDbContext)dbContextConstructor.Invoke(new object[] { connectionString });
            this.Store = new EventStore<TAggregate>(this.NodeName, serializer, this.EventStoreDbContextFactory, this.Time, this.Guid, this.Log);

            using (var context = this.EventStoreDbContextFactory.Invoke(false))
            {
                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = this.NodeName,
                    Url = "localhost",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationDate = this.Time.Now,
                    UpdateTime = this.Time.Now
                });

                context.SaveChanges();
            }
        }

        public string NodeName { get; }

        public ITimeProvider Time { get; }

        public IGuidProvider Guid { get; }

        public BusStub Bus { get; }

        public ILogger Log { get; }

        public TProcessor Processor { get; private set; }

        public Func<bool, IEventStoreDbContext> EventStoreDbContextFactory { get; }

        public Func<TDbContext> ReadModelDbContextFactory { get; }

        public IEventStore<TAggregate> Store { get; }

        public void Setup(TProcessor processor) => this.Processor = processor;

        public void Setup<TDao>(TDao dao) where TDao : class => this.dao = dao;

        public void Then<TDao>(Action<TDao> thenPredicate) where TDao : class => thenPredicate.Invoke((TDao)this.dao);

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> Given(IEvent @event)
            => this.When(@event);

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> Given(params IEvent[] events)
        {
            foreach (var e in events)
                this.When(e);

            return this;
        }

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> When(IEvent @event)
        {
            ((Event)@event).StreamType = this.NodeName;
            ((Event)@event).EventId = this.Guid.NewGuid();
            this.Processor.Handle(new NewIncomingEvent(this.serializer.SerializeAndDeserialize(@event)));
            return this;
        }

        public void Then(Action<TDbContext> readModelQueryPredicate)
        {
            using (var context = this.ReadModelDbContextFactory.Invoke())
            {
                readModelQueryPredicate(context);
            }
        }
    }
}
