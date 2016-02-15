using EventCentric.EventSourcing;
using EventCentric.Handling;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using System;

namespace EventCentric.Utils.Testing
{
    public class EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext>
        where TAggregate : class, IEventSourced
        where TProcessor : HandlerOf<TAggregate>
        where TDbContext : IEventStoreDbContext
    {
        private object dao;
        private readonly ITextSerializer serializer;

        public EventDenormalizerTestHelper(string connectionString, Func<IBus, ILogger, IEventStore<TAggregate>, TProcessor> processorFactory = null)
        {
            this.serializer = new JsonTextSerializer();
            this.Bus = new BusStub();
            this.Log = new ConsoleLogger();
            this.Time = new UtcTimeProvider();
            this.Guid = new SequentialGuid();
            this.NodeName = StreamNameResolver.ResolveFullNameOf<TAggregate>();

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
                    Url = "self",
                    Token = "#token",
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = this.Time.Now,
                    UpdateLocalTime = this.Time.Now
                });

                context.SaveChanges();
            }

            if (processorFactory == null)
            {
                var processorConstructor = typeof(TProcessor).GetConstructor(new[] { typeof(IBus), typeof(ILogger), typeof(IEventStore<TAggregate>) });
                Ensure.CastIsValid(processorConstructor, "Type TProcessor must have a constructor with the following signature: .ctor(IBus, ILogger, IEventStore<TAggregate>)");
                this.Processor = (TProcessor)processorConstructor.Invoke(new object[] { this.Bus, this.Log, this.Store });
            }
            else
                this.Processor = processorFactory.Invoke(this.Bus, this.Log, this.Store);
        }

        public string NodeName { get; }

        public IUtcTimeProvider Time { get; }

        public IGuidProvider Guid { get; }

        public BusStub Bus { get; }

        public ILogger Log { get; }

        public TProcessor Processor { get; }

        public Func<bool, IEventStoreDbContext> EventStoreDbContextFactory { get; }

        public Func<TDbContext> ReadModelDbContextFactory { get; }

        public IEventStore<TAggregate> Store { get; }

        public void Setup<TDao>(TDao dao) where TDao : class => this.dao = dao;

        public void Then<TDao>(Action<TDao> thenPredicate) where TDao : class => thenPredicate.Invoke((TDao)this.dao);

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> Given(IEvent @event)
        {
            if (@event.TransactionId == default(Guid))
                @event.AsEventWithFixedTransactionId(this.Guid.NewGuid());

            return this.When(@event);
        }

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> Given(params IEvent[] events)
        {
            foreach (var e in events)
                this.When(e);

            return this;
        }

        public EventDenormalizerTestHelper<TAggregate, TProcessor, TDbContext> When(IEvent @event)
        {
            ((Message)@event).StreamType = this.NodeName;
            ((Message)@event).EventId = this.Guid.NewGuid();
            this.Processor.AdHocHandle(this.serializer.SerializeAndDeserialize(@event));
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
