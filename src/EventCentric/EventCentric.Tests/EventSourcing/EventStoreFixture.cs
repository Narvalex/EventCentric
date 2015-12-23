using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Repository;
using EventCentric.Repository.Mapping;
using EventCentric.Serialization;
using EventCentric.Tests.EventSourcing.Helpers;
using EventCentric.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EventCentric.Tests.EventSourcing
{
    public class GIVEN_store : TestConfig, IDisposable
    {
        protected string appStreamType;
        protected string streamType;
        protected SqlClientLite sql;
        protected EventStore<InventoryTestAggregate> sut;
        protected Func<bool, IEventStoreDbContext> contextFactory;

        public GIVEN_store()
        {
            this.streamType = NodeNameResolver.ResolveNameOf<InventoryTestAggregate>();
            this.appStreamType = $"App_{this.streamType}";
            this.sql = new SqlClientLite(defaultConnectionString);
            this.contextFactory = (isReadOnly) => new EventStoreDbContext(isReadOnly, defaultConnectionString);
            this.sut = new EventStore<InventoryTestAggregate>(
                NodeNameResolver.ResolveNameOf<InventoryTestAggregate>(),
                new JsonTextSerializer(),
                this.contextFactory,
                new UtcTimeProvider(),
                new SequentialGuid(),
                new ConsoleLogger());

            this.Dispose();

            using (var context = this.contextFactory.Invoke(false))
            {
                ((EventStoreDbContext)context).Database.Create();

                var now = DateTime.Now;

                context.Subscriptions.Add(new SubscriptionEntity
                {
                    StreamType = this.appStreamType,
                    Url = "self",
                    Token = string.Empty,
                    ProcessorBufferVersion = 0,
                    IsPoisoned = false,
                    WasCanceled = false,
                    CreationLocalTime = now,
                    UpdateLocalTime = now
                });

                context.SaveChanges();
            }
        }

        public void Dispose()
            => this.sql.DropDatabase();
    }

    [TestClass]
    public class WHEN_saving_an_aggregate : GIVEN_store
    {
        [TestMethod]
        [TestCategory(DbIntegrationCategory)]
        public void THEN_processed_message_and_published_event_are_persisted()
        {
            var streamId = Guid.NewGuid();
            var incomingCommand = new AddItems(5).AsInProcessMessage(Guid.NewGuid(), streamId).AsQueuedEvent(this.appStreamType, Guid.NewGuid(), 1, DateTime.UtcNow, DateTime.Now);
            var aggregate = new InventoryTestAggregate(streamId);
            aggregate.Handle((AddItems)incomingCommand);

            Assert.AreEqual(1, aggregate.PendingEvents.Count());

            this.sut.Save(aggregate, incomingCommand);

            var retrievedAggregate = this.sut.Find(streamId);
            var retrievedMemento = retrievedAggregate.SaveToMemento();

            Assert.IsNotNull(retrievedAggregate);
            Assert.AreEqual(5, ((InventoryTestAggregateMemento)retrievedMemento).Quantity);
        }
    }
}
